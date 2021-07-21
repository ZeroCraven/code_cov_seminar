using System;
using System.Collections.Specialized;
using System.Threading;

namespace chargebook.data {
    public class ManagerPersistor : IDisposable {
        private readonly Thread persistenceThread;
        private bool running = true;
        private readonly Semaphore unsavedChangesSemaphore = new Semaphore(0, 1);

        public ManagerPersistor(Action persistenceFunction) {
            persistenceThread = new Thread(() => {
                while (running) {
                    unsavedChangesSemaphore.WaitOne();
                    if (!running) continue;
                    persistenceFunction();
                }
            });
            persistenceThread.Start();
        }

        public void notifyChange() {
            try {
                unsavedChangesSemaphore.Release();
            }
            catch (SemaphoreFullException) { }
        }

        public void Dispose() {
            running = false;
            try {
                unsavedChangesSemaphore.Release();
            }
            catch (SemaphoreFullException) { }
            persistenceThread.Join();
        }
    }
}