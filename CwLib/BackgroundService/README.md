# CwLib.BackgroundService

Create class

```cs
public class TestProcess : IBackgroundProcess
{
    /// <summary>
    /// Background Start
    /// </summary>
    public void BackgroundStart()
    {
       _task = Task.Run(() => { DoSomething(); });
    }

    Task _task;

    void DoSomething()
    {
        while(true)
        {

        }
    }

    /// <summary>
    /// Background Stop.
    /// </summary>
    public void BackgroundStop()
    {
        //Stop task...
    }
}
```

New instance and add to Process

```cs

    var testProcess = new TestProcess(); // new a instance

    var id = Process.Add(new TestProcess()); //add and start process...

    Process.Stop(id); //stop process...

```

Create process package

```cs
    var testProcess1 = new TestProcess();
    var testProcess2 = new TestProcess();

    var package = Process.NewPackage();

    package.Add(testProcess1);
    package.Add(testProcess2);

    package.Stop();
```