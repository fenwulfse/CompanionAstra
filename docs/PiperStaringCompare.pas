{
    Surgical Compare Script for Companion Scenes
    Focus: VMAD, PNAM, and Dialogue Flags
}
unit PiperStaringCompare;

var
  sl: TStringList;

function Initialize: Integer;
begin
  sl := TStringList.Create;
  sl.Add('RECORD_DUMP_START');
end;

function Process(e: IInterface): Integer;
var
  vmad, pnam, flags: string;
begin
  if Signature(e) = 'QUST' then begin
    sl.Add('--- QUEST: ' + EditorID(e) + ' ---');
    sl.Add('FormID: ' + IntToHex(FixedFormID(e), 8));
    sl.Add('Flags: ' + IntToStr(GetElementNativeValues(e, 'Record Header\Record Flags')));
    
    // Check for VMAD (Scripts)
    if HasElement(e, 'VMAD') then
      sl.Add('VMAD_Present: YES')
    else
      sl.Add('VMAD_Present: NO');
  end;

  if Signature(e) = 'SCEN' then begin
    sl.Add('--- SCENE: ' + EditorID(e) + ' ---');
    sl.Add('ParentQuest: ' + IntToHex(GetElementNativeValues(e, 'PNAM'), 8));
    sl.Add('Flags: ' + IntToStr(GetElementNativeValues(e, 'DNAM\Flags')));
  end;

  if Signature(e) = 'INFO' then begin
    sl.Add('--- DIALOG_INFO: ' + IntToHex(FixedFormID(e), 8) + ' ---');
    sl.Add('ResponseFlags: ' + IntToStr(GetElementNativeValues(e, 'TRDA\Unknown')));
  end;
end;

function Finalize: Integer;
begin
  sl.SaveToFile(ProgramPath + 'PiperCompareOutput.txt');
  sl.Free;
  AddMessage('Dump complete! Check your FO4Edit folder for PiperCompareOutput.txt');
end;

end.
