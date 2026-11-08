!include <stdverbs.lib>

define game <Error Names Test>
	asl-version <410>
	start <room>
	error <badcommand; Eh? Never heard of that.>
	error <defaulttake; Got it, thanks.>
	error <nosucherror; This message has nowhere to go.>
end define

define room <room>

	define object <ball>
		take
	end define

end define
