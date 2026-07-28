{
    Deep Staring Compare Script
    Compares COMPiper (Vanilla) vs custom Companion Quest
    Focus: Finding the "No Greeting Required" hook
}
unit DeepSceneCompare;

var
  sl: TStringList;

function Initialize: Integer;
begin
  sl := TStringList.Create;
  sl.Add('=== DEEP SCENE TRUTH TABLE ===');
  sl.Add('Generated: ' + DateTimeToStr(Now));
  sl.Add('');
end;

procedure CompareRecords(e1, e2: IInterface; LabelStr: string);
var
  v1, v2: string;
begin
  v1 := IntToHex(GetElementNativeValues(e1, ''), 8);
  v2 := IntToHex(GetElementNativeValues(e2, ''), 8);
  sl.Add(Format('%-30s | Piper: %-10s | Claude: %-10s | Match: %s', [LabelStr, v1, v2, BoolToStr(v1 = v2, True)]));
end;

function Process(e: IInterface): Integer;
var
  i, j: Integer;
  scenes, phases, actions: IInterface;
  sID: string;
begin
  if Signature(e) <> 'QUST' then Exit;
  
  sl.Add('--- ANALYZING QUEST: ' + EditorID(e) + ' ---');
  sl.Add('Priority: ' + IntToStr(GetElementNativeValues(e, 'Data\Priority')));
  sl.Add('Flags:    ' + IntToHex(GetElementNativeValues(e, 'Record Header\Record Flags'), 8));
  
  scenes := ElementBySignature(e, 'SCEN'); // This logic is simplified for the dump
  
  // We'll iterate through all scenes in the quest
  scenes := ElementByName(e, 'Scenes');
  for i := 0; i < ElementCount(scenes) do begin
    sID := EditorID(LinksTo(ElementByIndex(scenes, i)));
    sl.Add('');
    sl.Add('  >> SCENE: ' + sID);
    sl.Add('  DNAM Flags: ' + IntToHex(GetElementNativeValues(LinksTo(ElementByIndex(scenes, i)), 'DNAM'), 8));
    
    phases := ElementByName(LinksTo(ElementByIndex(scenes, i)), 'Phases');
    sl.Add('  Phases Count: ' + IntToStr(ElementCount(phases)));
    
    for j := 0; j < ElementCount(phases) do begin
      sl.Add('    Phase ' + IntToStr(j) + ' SetStage: ' + IntToStr(GetElementNativeValues(ElementByIndex(phases, j), 'Phase Set Parent Quest Stage\On End')));
      if HasElement(ElementByIndex(phases, j), 'Conditions') then
        sl.Add('    Phase ' + IntToStr(j) + ' Conditions: YES')
      else
        sl.Add('    Phase ' + IntToStr(j) + ' Conditions: NO');
    end;
  end;
  sl.Add('------------------------------------------------');
end;

function Finalize: Integer;
begin
  sl.SaveToFile(ProgramPath + 'DeepCompareResult.txt');
  sl.Free;
  AddMessage('Staring Compare Complete! File: DeepCompareResult.txt');
end;

end.
