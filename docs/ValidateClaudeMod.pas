unit ValidateClaudeMod;

// xEdit/FO4Edit validation script for CompanionClaude.esp
// Run via: Right-click in left pane → Apply Script → ValidateClaudeMod

interface

implementation

uses
  wbInterface, wbHelpers;

var
  esp: IwbFile;
  errors: TStringList;
  warnings: TStringList;

procedure AddError(msg: string);
begin
  errors.Add('[ERROR] ' + msg);
  AddMessage('[ERROR] ' + msg);
end;

procedure AddWarning(msg: string);
begin
  warnings.Add('[WARN] ' + msg);
  AddMessage('[WARN] ' + msg);
end;

function GetStageFragment(quest: IInterface; stageNum: integer): IInterface;
var
  stages, stg: IInterface;
  i: integer;
begin
  Result := nil;
  stages := ElementByPath(quest, 'Stages');
  if not Assigned(stages) then Exit;

  for i := 0 to ElementCount(stages) - 1 do begin
    stg := ElementByIndex(stages, i);
    if GetElementNativeValues(stg, 'INDX - Stage Index') = stageNum then begin
      Result := stg;
      Exit;
    end;
  end;
end;

procedure ValidateQuestStage(quest: IInterface; stageNum: integer; expectedFlags: integer; shouldHaveFragment: boolean);
var
  stage, vmad: IInterface;
  flags: integer;
begin
  if not Assigned(quest) then begin
    AddError(Format('Quest not assigned when checking stage %d', [stageNum]));
    Exit;
  end;

  stage := GetStageFragment(quest, stageNum);

  if not Assigned(stage) then begin
    AddError(Format('Quest stage %d not found', [stageNum]));
    Exit;
  end;

  flags := GetElementNativeValues(stage, 'INDX\Stage Flags');
  if flags <> expectedFlags then
    AddWarning(Format('Stage %d has flags %d, expected %d', [stageNum, flags, expectedFlags]));

  vmad := ElementByPath(stage, 'VMAD');
  if shouldHaveFragment and not Assigned(vmad) then
    AddError(Format('Stage %d missing VMAD fragment script', [stageNum]));
  if not shouldHaveFragment and Assigned(vmad) then
    AddWarning(Format('Stage %d has unexpected VMAD fragment', [stageNum]));
end;

procedure ValidateScene(sceneName: string; expectedPhases: integer; expectedEndStage: integer);
var
  scene, phases, lastPhase, stageInfo: IInterface;
  phaseCount, endStage: integer;
begin
  scene := MainRecordByEditorID(GroupBySignature(esp, 'SCEN'), sceneName);

  if not Assigned(scene) then begin
    AddError(Format('Scene "%s" not found', [sceneName]));
    Exit;
  end;

  phases := ElementByPath(scene, 'Phases');
  if not Assigned(phases) then begin
    AddError(Format('Scene "%s" has no phases', [sceneName]));
    Exit;
  end;

  phaseCount := ElementCount(phases);
  if phaseCount <> expectedPhases then
    AddError(Format('Scene "%s" has %d phases, expected %d', [sceneName, phaseCount, expectedPhases]));

  lastPhase := ElementByIndex(phases, phaseCount - 1);
  stageInfo := ElementByPath(lastPhase, 'PHAS\End Phase Set Stage');

  if Assigned(stageInfo) then begin
    endStage := GetNativeValue(stageInfo);
    if endStage <> expectedEndStage then
      AddError(Format('Scene "%s" sets stage %d, expected %d (TEST CHAIN BROKEN)', [sceneName, endStage, expectedEndStage]));
  end else begin
    AddWarning(Format('Scene "%s" does not set end stage', [sceneName]));
  end;
end;

procedure ValidateGreeting(topicEditorID: string; expectedWants: integer; expectedStageDone: integer; expectedNotStageDone: integer);
var
  topic, responses, response, conditions, cond: IInterface;
  i, func, compVal: integer;
  foundWants, foundStageDone, foundNotStageDone: boolean;
begin
  topic := MainRecordByEditorID(GroupBySignature(esp, 'DIAL'), topicEditorID);
  if not Assigned(topic) then begin
    AddError(Format('Topic "%s" not found', [topicEditorID]));
    Exit;
  end;

  responses := ElementByPath(topic, 'Responses');
  if not Assigned(responses) or (ElementCount(responses) = 0) then begin
    AddError(Format('Topic "%s" has no responses', [topicEditorID]));
    Exit;
  end;

  response := ElementByIndex(responses, 0);
  conditions := ElementByPath(response, 'Conditions');

  foundWants := false;
  foundStageDone := false;
  foundNotStageDone := false;

  if Assigned(conditions) then begin
    for i := 0 to ElementCount(conditions) - 1 do begin
      cond := ElementByIndex(conditions, i);
      func := GetElementNativeValues(cond, 'CTDA\Function');
      compVal := GetElementNativeValues(cond, 'CTDA\Comparison Value');

      // Function 214 = GetVMQuestVariable, Function 58 = GetStageDone
      if func = 214 then begin // WantsToTalk check
        if compVal = expectedWants then
          foundWants := true;
      end;

      if func = 58 then begin // GetStageDone
        if (GetElementEditValues(cond, 'CTDA\Comparison Value') = '1.000000') then begin
          if compVal = expectedStageDone then
            foundStageDone := true;
        end;
        if (GetElementEditValues(cond, 'CTDA\Comparison Value') = '0.000000') then begin
          if compVal = expectedNotStageDone then
            foundNotStageDone := true;
        end;
      end;
    end;
  end;

  if not foundWants then
    AddError(Format('Greeting "%s" missing WantsToTalk=%d condition', [topicEditorID, expectedWants]));
  if (expectedStageDone > 0) and not foundStageDone then
    AddError(Format('Greeting "%s" missing StageDone=%d condition', [topicEditorID, expectedStageDone]));
  if (expectedNotStageDone > 0) and not foundNotStageDone then
    AddError(Format('Greeting "%s" missing NOT StageDone=%d exclusion', [topicEditorID, expectedNotStageDone]));
end;

function Initialize: integer;
var
  quest, questGroup, stg, elem: IInterface;
  i, j, nativeVal: integer;
  stageNum, editVal: string;
begin
  Result := 0;
  errors := TStringList.Create;
  warnings := TStringList.Create;

  AddMessage('========================================');
  AddMessage('CompanionClaude.esp Validation Script');
  AddMessage('========================================');
  AddMessage('');

  esp := FileByName('CompanionClaude.esp');

  if not Assigned(esp) then begin
    AddMessage('[FATAL] CompanionClaude.esp not loaded!');
    Result := 1;
    Exit;
  end;

  AddMessage('DEBUG: ESP loaded: ' + GetFileName(esp));

  // Debug: List all quests in the ESP
  questGroup := GroupBySignature(esp, 'QUST');
  if Assigned(questGroup) then begin
    AddMessage(Format('DEBUG: Found %d QUST records', [ElementCount(questGroup)]));
    for i := 0 to ElementCount(questGroup) - 1 do begin
      quest := ElementByIndex(questGroup, i);
      AddMessage('  - ' + EditorID(quest));
    end;
  end else begin
    AddMessage('DEBUG: No QUST group found!');
  end;

  // Find the main quest
  quest := MainRecordByEditorID(GroupBySignature(esp, 'QUST'), 'COMClaude');
  if not Assigned(quest) then begin
    AddError('Main quest "COMClaude" not found in ESP!');
    Result := 1;
    Exit;
  end;

  AddMessage('');
  AddMessage('DEBUG: Quest stages in COMClaude:');

  // Debug: List all stages
  questGroup := ElementByPath(quest, 'Stages');
  if Assigned(questGroup) then begin
    AddMessage(Format('  Found %d stages', [ElementCount(questGroup)]));

    // Debug first few stages with multiple methods
    for i := 0 to 9 do begin
      if i >= ElementCount(questGroup) then Break;
      stg := ElementByIndex(questGroup, i);

      // Try getting INDX element directly
      elem := ElementByName(stg, 'INDX - Stage Index');
      if Assigned(elem) then begin
        nativeVal := GetNativeValue(elem);
        editVal := GetEditValue(elem);
        AddMessage('    Stage[' + IntToStr(i) + ']: Native=' + IntToStr(nativeVal) + ', Edit="' + editVal + '", Name="' + Name(stg) + '"');
      end else begin
        AddMessage('    Stage[' + IntToStr(i) + ']: INDX element not found, Name="' + Name(stg) + '"');
      end;
    end;
  end else begin
    AddMessage('  No stages found!');
  end;

  AddMessage('');
  AddMessage('Validating quest stages...');
  ValidateQuestStage(quest, 110, 0, true);
  ValidateQuestStage(quest, 406, 0, true);
  ValidateQuestStage(quest, 410, 0, true);
  ValidateQuestStage(quest, 440, 0, true);
  ValidateQuestStage(quest, 496, 0, true);

  AddMessage('');
  AddMessage('Validating scenes...');
  ValidateScene('COMClaude_01_DismissToFriendship', 8, 406);
  ValidateScene('COMClaude_02_FriendshipToAdmiration', 6, 410);
  ValidateScene('COMClaude_02a_AdmirationToConfidant', 8, 440);
  ValidateScene('COMClaude_03_AdmirationToInfatuation', 14, 496);

  AddMessage('');
  AddMessage('Validating greeting conditions...');
  ValidateGreeting('COMClaudeGreetings', 2, 0, 406);     // Friendship: Wants=2, NOT StageDone 406
  ValidateGreeting('COMClaudeGreetings', 2, 406, 410);   // Admiration: Wants=2, StageDone 406, NOT 410
  ValidateGreeting('COMClaudeGreetings', 2, 410, 440);   // Confidant: Wants=2, StageDone 410, NOT 440
  ValidateGreeting('COMClaudeGreetings', 2, 440, 496);   // Infatuation: Wants=2, StageDone 440, NOT 496

  AddMessage('');
  AddMessage('========================================');
  AddMessage('Validation Complete');
  AddMessage('========================================');
  AddMessage(Format('Errors: %d', [errors.Count]));
  AddMessage(Format('Warnings: %d', [warnings.Count]));

  if errors.Count = 0 then
    AddMessage('✓ PASS: All critical checks passed')
  else
    AddMessage('✗ FAIL: Critical errors found');

  errors.Free;
  warnings.Free;
end;

end.
