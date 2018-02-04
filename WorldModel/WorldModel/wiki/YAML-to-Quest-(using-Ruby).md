This is a quick way to generate a basic map. You still have to type the name and description for each location, but you can do that for a load of rooms in one file, rather than flicking between different dialogues. Whether you think that is useful, is up to you.

You will need Ruby installed on your PC. I use JRuby, which is available [here](http://jruby.org/download).

The text must go in a file called "quest.yml", and formatted in a specific way (called YAML). Here is an example:

```yaml
- City

- Name: Market Square
  Desc: The market square is a large open area, currently devoid of any market.
  
- Name: Alchemy Shop
  Inside: true
  Desc: This is a quaint shop, with a frankly bizarre smell.
  South: Market Square
```

It is assumed that this is for a big project, and it is a good idea to organise your game so all locations in a certain region are themelves inside a zone room (which the player cannot access). The above is for two rooms, a shop and a market square, inside a zone called "City". 

### More on YAML

In YAML, each entry in a list starts with a hyphen and a space.

The first entry must be the name of the zone, as show above. The following entries will go into that zone, until a new zone entry is encountered, at which point the new zone will be created and rooms will go into that zone. You can have as many zones and as many rooms in a zone as you like.

The other two entries in the example describe rooms. Note that the attributes have to be aligned, so the first line has a hyphen and a space, and subsequent lines have two spaces. This is important because YAML determines what belongs to what by its alignment.

 Each room entry has a set of attributes: "Name", a "Desc" (description), and any number of exits. Each attribute name must start with a capital, and end with a colon. You can put text processor commands in the description, but not at the very start.

Exits are assumed to go both ways. You should only specify one, and it must be to a room high in the list, as seen above, which will give a exit south from the shop to the market square, and another north back again. The name has to match exactly.

You can give a room an "Inside" attribute, to flag that it is inside.

You can also give an "Objs" attribute to put objects in the room. These objects will all be flagged as scenery. You must format the objects like this:

```YAML
- Name: Alchemy Shop
  Inside: true
  Desc: This is a quaint shop, with a frankly bizarre smell.
  South: Market Square
  Objs:
    herbs: You are not sure if the herbs are the source of the smell, or are supposing to be disguising it.
    bottles: You look over the bottles, reading the labels. There are cures for all sorts of things.
```

The objects themselves belong to the "Objs" attribute, so each object needs exactly four spaces at the start of the line. Then the name of the object, a colon, a space, and then the description.


### Converting to Quest

Now you need the program to convert it.

```ruby
# This will be added to every object and room
# Useful for a group project to avoid name collisions
OBJECT_PREFIX = ""



require 'yaml'

LIST1 = "North;Northeast;East;Southeast;South;Southwest;West;NorthWest;Up;Down;In;Out".split ";"
LIST2 = "South;Southwest;West;NorthWest;North;Northeast;East;Southeast;Down;Up;Out;In".split ";"


class Room
  attr_reader :alias, :desc, :name, :exits, :inside

  def initialize name, desc
    @alias = name
    @desc = desc
    @name = name.gsub(' ', '_').gsub(/[^\w]/, '').downcase
    # p @alias
    @exits = []
    @objects = []
  end
  
  def inside
    @inside = true
  end

  def to_s
    s = "    <object name=\"#{OBJECT_PREFIX}#{@name}\">\n"
    s += "      <inherit name=\"editor_room\" />\n"
    s += "      <alias>#{@alias}</alias>\n"
    s += "      <description><![CDATA["
    s += @desc
    s += "]]></description>\n"
    s += "      <inside />\n" if @inside
    @objects.each do |k, v|
      s += "      <object name=\"#{OBJECT_PREFIX}#{k.gsub(' ', '_').gsub(/[^\w]/, '').downcase}\">\n"
      s += "        <inherit name=\"editor_object\" />\n"
      s += "        <alias>#{k}</alias>\n"
      s += "        <scenery/>\n"
      s += "        <description><![CDATA["
      s += v
      s += "]]></description>\n"
      s += "      </object>\n"
    end
    @exits.each { |exit| s += exit.to_s }
    s += "    </object>\n"
    s
  end
  
  def objects h
    @objects = h
  end

end

class Exit
  attr_reader :to, :dir
  
  def initialize dir, to, message = nil
    @to = to
    @message = message if message != ''
    @dir = dir
  end
  
  def to_s
    s = "    <exit alias=\"#{@dir.downcase}\" to=\"#{OBJECT_PREFIX}#{@to.name}\">\n"
    s += "      <inherit name=\"#{@dir.downcase}direction\" />\n"
    s += "      <message>#{@message}</message>\n" if @message
    s += "    </exit>\n"
    s
  end

end  



def find list, name
  l = list.select { |e| e.is_a?(Room) && e.alias == name }
  raise "Not found #{name}" if l.length == 0
  raise "Found multiple #{name}" if l.length > 1
  l[0]
end


def reverse dir
  n = LIST1.index dir
  raise "Direction not found #{dir}" unless n
  LIST2[n]
end    



data = YAML.load_file("quest.yml")
list = []
data.each do |h|
  if h.is_a? Hash
    # p h
    room = Room.new h['Name'], h['Desc']
    h.delete 'Name'
    h.delete 'Desc'
    if h['Inside']
      room.inside
      h.delete 'Inside'
    end
    if h['Objs']
      room.objects h['Objs']
      h.delete 'Objs'
    end
    # p h
    h.each do |dir, v|
      l = v.split '|'
      s1 = l.length > 1 ? l[1] : nil
      s2 = l.length > 2 ? l[2] : nil
      dest = find(list, l[0])
      room.exits << Exit.new(dir, dest, s1)
      dest.exits << Exit.new(reverse(dir), room, s2)
    end
    list << room
  else
    list << h
  end
end



File.open("quest.xml", "w") do |file|
  flag = false
  list.each do |el|
    if el.is_a? Room
      file << el.to_s
    else
      file << "  </object>\n" if flag
      file << "  <object name=\"#{OBJECT_PREFIX}#{el.gsub(' ', '_').gsub(/[^\w]/, '').downcase}\">\n"
      file << "    <inherit name=\"editor_room\" />\n"
      flag = true
    end
  end
  file << "  </object>\n"
end

print "Done"
```

Put the code in a text file called YamlToQuest.rb, in the same folder as quest.yml. Once you have Ruby installed you can do:

```
ruby YamlToQuest.rb
```

This will produce a file, quest.xml. Open it up, and paste the XML into you game (it needs to go in after this line):

```
  </game>
```


### Object prefixes

You can add a prefix to every object generated (this would be useful if you are working on a group project and you need to demarcate your objects to avoid a name conflict). You will need to change the Ruby code, so open YamlToQuest.rb wih a text editor, and look for this line (it is pretty much at the top):

```
OBJECT_PREFIX = ""
```

Put your desired prefix inside the quotes, for instance like this:

```
OBJECT_PREFIX = "px_"
```
