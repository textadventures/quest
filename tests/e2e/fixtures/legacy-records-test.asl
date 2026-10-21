!include <stdverbs.lib>

define game <Records Test>
	asl-version <410>
	start <room>
end define

define room <room>

	command <removeloose> {
		remove <loose>
		msg <Removed.>
	}

	command <missingprop> msg <Before #nosuch:colour# after>

	command <clonetwice> {
		clone <tomato; tomato2; room>
		clone <tomato; tomato3; room>
		property <tomato2; colour=green>
		msg <Colours: #tomato:colour# #tomato2:colour# #tomato3:colour#>
	}

	define object <box>
		properties <container; opened; seen; remove>
	end define

	define object <ball>
		properties <parent=box>
		take
	end define

	define object <loose>
	end define

	define object <tomato>
		properties <colour=red>
	end define

end define
