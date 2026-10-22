!include <stdverbs.lib>

define game <Room Array Test>
	asl-version <410>
	start <room>

	define variable <n>
		type numeric
		value <2>
	end define
end define

define room <room>
	command <gocell> goto <cell[n]>
	command <movecoin> {
		move <coin; cell[n]>
		goto <cell2>
	}

	define object <coin>
	end define
end define

define room <cell1>
end define

define room <cell2>
end define
