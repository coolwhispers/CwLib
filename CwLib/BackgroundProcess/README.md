# CwLib.BackgroundService

Create class

```cs
public class TestProcess : IBackgroundProcess
{
    /// <summary>
    /// Start DoSomething()
    /// </summary>
    public void BackgroundStart()
    {
        _run = true;
       _task = Task.Run(() => { DoSomething(); });
    }

    Task _task;
    bool _run;

    void DoSomething()
    {
        while(_run)
        {
            // Do something...
        }
    }

    /// <summary>
    /// Stop Dosomething()
    /// </summary>
    public void BackgroundStop()
    {
        //Stop DoSomething() task...
        _run =false;
        _task.Wait();
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