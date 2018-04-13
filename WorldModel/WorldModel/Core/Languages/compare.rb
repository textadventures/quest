require "rexml/document"
include REXML






def get_lists filename
  doc = REXML::Document.new File.new(filename)
  templates = []
  doc.elements.each("library/template") do |temp|
    templates << temp.attributes["name"]
  end
  dynamic_templates = []
  doc.elements.each("library/dynamictemplate") do |temp|
    dynamic_templates << temp.attributes["name"]
  end
  verb_templates = []
  doc.elements.each("library/verbtemplate") do |temp|
    verb_templates << temp.attributes["name"]
  end
  return templates, dynamic_templates, verb_templates
end



a1, b1, c1 = get_lists "English.aslx"
a2, b2, c2 = get_lists "Italiano.aslx"
p (a1 - a2)
p (b1 - b2)
p (c1 - c2)
