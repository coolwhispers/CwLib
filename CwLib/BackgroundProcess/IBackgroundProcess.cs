namespace CwLib.BackgroundProcess
{
    public interface IBackgroundProcess
    {
        /// <summary>
        /// Background Start
        /// </summary>
        void BackgroundStart();

        /// <summary>
        /// Background Stop.
        /// </summary>
        void BackgroundStop();
    }
}