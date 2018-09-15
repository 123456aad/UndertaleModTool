// Adds room ID and name display under the debug mode timer display

EnsureDataLoaded();

ScriptMessage("Show room name and ID in debug mode\nby krzys_h");

var code = Data.GameObjects.ByName("obj_time").EventHandlerFor(EventType.Draw, EventSubtypeDraw.DrawGUI, Data.Strings, Data.Code);

code.Append(Assembler.Assemble(@"
; if debug disabled, jump to end
pushglb.v global.debug@395
pushi.e 1
cmp.i.v EQ
bf func_end

; draw room id at x=10 y=30
push.i " + 0xFFFF00.ToString() /* TODO: add this syntax to the assembler */ + @"
conv.i.v
call.i draw_set_color(argc=1)
popz.v
pushvar.v room@23
pushi.e 30
conv.i.v
pushi.e 10
conv.i.v
call.i draw_text(argc=3)
popz.v

; draw room name at x=50 y=30
pushvar.v room@23
call.i room_get_name(argc=1)
pushi.e 30
conv.i.v
pushi.e 50
conv.i.v
call.i draw_text(argc=3)
popz.v
", Data.Functions, Data.Variables, Data.Strings));

ChangeSelection(code);
ScriptMessage("Patched!");