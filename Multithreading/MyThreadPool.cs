using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multithreading
{
    public delegate void WorkDelegate(object WorkObject);

    public class MyThreadPool
    {
        public delegate void WorkDelegate(object WorkObject);
        private int m_MinThreads = 0;
        private int m_MaxThreads = 100;
        private int m_IdleTimeThreshold = 5;
        private Queue<WorkItem> WorkQueue = new Queue<WorkItem>();
        private List<WorkThread> ThreadList = new List<WorkThread>();
        private Thread? ManagementThread;
        private bool KeepManagementThreadRunning = true;

        class WorkItem
        {
            public object? WorkObject;
            public WorkDelegate? Delegate;
        }

        public int MinThreads
        {
            get
            {
                return this.m_MinThreads;
            }
            set
            {
                this.m_MinThreads = value;
            }
        }

        public int MaxThreads
        {
            get
            {
                return this.m_MaxThreads;
            }
            set
            {
                this.m_MaxThreads = value;
            }
        }

        public int IdleTimeThreshold
        {
            get
            {
                return this.m_IdleTimeThreshold;
            }
            set
            {
                this.m_IdleTimeThreshold = value;
            }
        }

        public int QueueLength
        {
            get
            {
                return WorkQueue.Count();
            }
        }

        public void QueueWork(object WorkObject, WorkDelegate Delegate)
        {
            WorkItem wi = new WorkItem();
            wi.WorkObject = WorkObject;
            wi.Delegate = Delegate;

            lock (WorkQueue)
            {
                WorkQueue.Enqueue(wi);
            }

            //Now see if there are any threads that are idle
            bool FoundIdleThread = false;
            foreach (WorkThread wt in ThreadList)
            {
                if (!wt.Busy)
                {
                    wt.WakeUp();
                    FoundIdleThread = true; break;
                }
            }

            if (!FoundIdleThread)
            {
                //See if we can create a new thread to handle the
                //additional workload
                if (ThreadList.Count < m_MaxThreads)
                {
                    WorkThread wt = new WorkThread(ref WorkQueue);
                    lock (ThreadList)
                    {
                        ThreadList.Add(wt);
                    }
                }
            }
        }

        private void ManagementWorker()
        {
            while (KeepManagementThreadRunning)
            {
                try
                {
                    //Check to see if we have idle thread we should free up
                    if (ThreadList.Count > m_MinThreads)
                    {
                        foreach (WorkThread wt in ThreadList)
                        {
                            if (DateTime.Now.Subtract(wt.LastOperation).Seconds
                               > m_IdleTimeThreshold)
                            {
                                wt.ShutDown();
                                lock (ThreadList)
                                {
                                    ThreadList.Remove(wt); break;
                                }
                            }
                        }
                    }
                }
                catch { }

                try
                {
                    Thread.Sleep(1000);
                }
                catch { }
            }
        }

        class WorkThread
        {
            public Queue<WorkItem> m_WorkQueue;
            public bool m_KeepRunning = true;
            public DateTime m_LastOperation;
            public bool m_Busy;
            public WorkThread(ref Queue<WorkItem> m_WorkQueue)
            {
                this.m_WorkQueue = m_WorkQueue;

                if (this.m_WorkQueue.Count > 0)
                {
                    m_KeepRunning = true;
                }
                else
                {
                    m_KeepRunning = false;
                }

                Worker();
            }

            public DateTime LastOperation
            {
                get { return m_LastOperation; }
                set { m_LastOperation = value; }
            }

            public bool Busy
            {
                get { return m_Busy; }
                set { m_Busy = value; }
            }

            private void Worker()
            {
                WorkItem wi;
                while (m_KeepRunning)
                {
                    try
                    {
                        while (m_WorkQueue.Count > 0)
                        {
                            wi = null;
                            lock (m_WorkQueue)
                            {
                                wi = m_WorkQueue.Dequeue();
                            }
                            if (wi != null)
                            {
                                m_LastOperation = DateTime.Now;
                                m_Busy = true;
                                wi.Delegate.Invoke(wi.WorkObject);
                            }
                        }
                    }
                    catch { }

                    try
                    {
                        m_KeepRunning = false;
                        Thread.Sleep(1000);   
                    }
                    catch { }
                }
            }

            public void ShutDown()
            {
                this.m_KeepRunning = false;
            }

            public void WakeUp()
            {
                this.m_KeepRunning = true;
            }
        }

    }
}
