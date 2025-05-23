using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StudyData", menuName = "Study/StudyData")]
public class StudyData : ScriptableObject
{
    public readonly int NumberOfPathsPerBlock = 3;
    public int ParticipantNumber = 1;
    public int NumberCompletedParticipants = 0;
    public int OverallTrialNumber = 1;
    public int BlockNumber = 1;
    public int TrialInBlock = 1;
    public PathSet PathSet;
    public PathData TrialPath;
    public LocomotionMethod LocomotionMethod = LocomotionMethod.Walking;
    public PrimaryHand PrimaryHand = PrimaryHand.Right;
    public List<PathSet> PathSetOrder;
    public List<LocomotionMethod> LocomotionOrder;
    [HideInInspector] public float TotalStudyTimeSec = 0f;

    public void LoadStudyData(string participantNumber, string overallTrialNumber,
        string blockNumber, string trialInBlock, string pathSetName, string pathName, string pathDifficulty,
        string locomotionMethod, string primaryHand, string navigationTimeSec, string backtrackTimeSec,
        string assessmentTimeSec, string trialTimeSec, string studyTimeSec, string studyDate, string systemTime)
    {
        this.ParticipantNumber = int.Parse(participantNumber);
        this.OverallTrialNumber = int.Parse(overallTrialNumber);
        this.BlockNumber = int.Parse(blockNumber);
        this.TrialInBlock = int.Parse(trialInBlock);
        this.PathSet = ResourceManager.Instance.GetPathSet(pathSetName);
        this.TrialPath = ResourceManager.Instance.GetPathData(pathName);
        this.LocomotionMethod = (LocomotionMethod)Enum.Parse(typeof(LocomotionMethod), locomotionMethod);
        this.PrimaryHand = (PrimaryHand)Enum.Parse(typeof(PrimaryHand), primaryHand);
        this.TotalStudyTimeSec = float.Parse(studyTimeSec);

        LoadLocomotionAndSetOrder(int.Parse(participantNumber));
    }

    public void SetStudyData(int blockNumber, int trialInBlock, PathSet pathSet, PathData path,
        LocomotionMethod locomotionMethod, PrimaryHand primaryHand)
    {
        this.BlockNumber = blockNumber;
        this.TrialInBlock = trialInBlock;
        this.PathSet = pathSet;
        this.TrialPath = path;
        this.LocomotionMethod = locomotionMethod;
        this.PrimaryHand = primaryHand;

        OverallTrialNumber = trialInBlock + (blockNumber - 1) * NumberOfPathsPerBlock;

        DataManager.Instance.SetBaseParticipantData(this);
    }

    public void PrepareNextTrial()
    {
        TrialInBlock++;
        OverallTrialNumber++;

        if (TrialInBlock > PathSet.PathList.Count)
        {
            Debug.Log($"Preparing new study block");
            TrialInBlock = 1;
            BlockNumber++;

            if (BlockNumber > Enum.GetValues(typeof(LocomotionMethod)).Length)
            {
                Debug.Log($"Resetting study for new participant");
                if (! PrepareNewParticipant(ParticipantNumber + 1))
                {
                    return;
                }
            }
        }

        LocomotionMethod = LocomotionOrder[BlockNumber - 1];
        PathSet = PathSetOrder[BlockNumber - 1];
        TrialPath = PathSet.PathList[TrialInBlock - 1];

        OverallTrialNumber = TrialInBlock + (BlockNumber - 1) * NumberOfPathsPerBlock;

        Debug.Log($"Trail prepared: Participant: {ParticipantNumber} - Trail Number {OverallTrialNumber} (Block: {BlockNumber} - Trail {TrialInBlock}) \n Locomotion: {LocomotionMethod} (Set: {PathSet} - Path {TrialPath})");
    }


    public bool PrepareNewParticipant(int participantNumber)
    {
        ResetStudyData();
        if (! LoadLocomotionAndSetOrder(participantNumber))
        {
            return false;
        }
        // ParticipantData participantData = ResourceManager.Instance.GetParticipantData(participantNumber);
            // if (participantData == null)
            // {
            //     Debug.LogError($"There is no participant number {participantNumber}");
            //     return false;
            // }

            // participantData.pathSetOrder.ForEach(setName =>
            // {
            //     PathSet set = ResourceManager.Instance.GetPathSet(setName);
            //     PathSetOrder.Add(set);
            // });

            // participantData.locomotionOrder.ForEach(locomotion =>
            // {
            //     LocomotionMethod method = (LocomotionMethod)Enum.Parse(typeof(LocomotionMethod), locomotion);
            //     LocomotionOrder.Add(method);
            // });

            this.ParticipantNumber = participantNumber;

        LocomotionMethod = LocomotionOrder[BlockNumber - 1];
        PathSet = PathSetOrder[BlockNumber - 1];
        TrialPath = PathSet.PathList[TrialInBlock - 1];

        return true;
    }

    public bool LoadLocomotionAndSetOrder(int participantNumber)
    {
        ParticipantData participantData = ResourceManager.Instance.GetParticipantData(participantNumber);
        if (participantData == null)
        {
            Debug.LogError($"There is no participant number {participantNumber}");
            return false;
        }
        PathSetOrder.Clear();
        participantData.pathSetOrder.ForEach(setName =>
        {
            PathSet set = ResourceManager.Instance.GetPathSet(setName);
            PathSetOrder.Add(set);
        });

        LocomotionOrder.Clear();
        participantData.locomotionOrder.ForEach(locomotion =>
        {
            LocomotionMethod method = (LocomotionMethod)Enum.Parse(typeof(LocomotionMethod), locomotion);
            LocomotionOrder.Add(method);
        });

        return true;
    }

    private void ResetStudyData()
    {
        BlockNumber = 1;
        TrialInBlock = 1;
        OverallTrialNumber = 1;
        LocomotionOrder.Clear();
        LocomotionMethod = LocomotionMethod.Walking;
        PrimaryHand = PrimaryHand.Right;
        PathSet = null;
        PathSetOrder.Clear();
        TrialPath = null;
        TotalStudyTimeSec = 0f;

        // If study is currently running, stop it and initiate completion
        if (StudyManager.Instance.IsRunning)
        {
            StudyManager.Instance.StudyState = StudyState.StudyCompleted;
        }
    }
}
