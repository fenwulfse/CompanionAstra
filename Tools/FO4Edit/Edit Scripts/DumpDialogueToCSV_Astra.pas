unit DumpDialogueToCSV_Astra;

interface

implementation

uses
  wbInterface, wbHelpers;

var
  sl: TStringList;
  esp: IInterface;

function Initialize: integer;
begin
  Result := 0;
  sl := TStringList.Create;
  sl.Add('FormID,Topic,Text');

  esp := FileByName('CompanionAstra.esp');
  if not Assigned(esp) then begin
    AddMessage('[FATAL] CompanionAstra.esp not loaded!');
    Result := 1;
    Exit;
  end;
end;

function Process(e: IInterface): integer;
var
  topic, text, formid: string;
begin
  Result := 0;
  if Signature(e) <> 'INFO' then Exit;
  if GetFile(e) <> esp then Exit;

  topic := GetElementEditValues(e, 'Topic');
  text := GetElementEditValues(e, 'Responses\List\0\Text\Value');
  formid := IntToHex(GetLoadOrderFormID(e), 8);

  sl.Add(Format('%s,%s,"%s"', [
    formid,
    StringReplace(topic, ',', ';', [rfReplaceAll]),
    StringReplace(text, '"', '""', [rfReplaceAll])
  ]));
end;

function Finalize: integer;
begin
  sl.SaveToFile(ProgramPath + 'Astra_DialogueDump.csv');
  sl.Free;
  AddMessage('Dump complete: Astra_DialogueDump.csv');
  Result := 0;
end;

end.
