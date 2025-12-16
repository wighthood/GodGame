public interface ISaveable
{
    string CaptureState();
    void RestoreState(string _state);
    string GetSaveID();
}
