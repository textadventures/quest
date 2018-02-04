# A way to convert YAML to Quest XML
#
# Version 2: added scenery objects and zones
# Version 3: added object prefixes
#
# By The Pixie


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