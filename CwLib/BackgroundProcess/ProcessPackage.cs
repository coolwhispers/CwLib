using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CwLib.BackgroundProcess
{
    /// <summary>
    /// Process List
    /// </summary>
    /// <seealso cref="System.Collections.IEnumerable" />
    public class ProcessPackage : IEnumerable
    {
        internal static ProcessPackage Create()
        {
            return new ProcessPackage();
        }

        private ProcessPackage()
        {
        }

        private List<Guid> _serivceIds = new List<Guid>();

        /// <summary>
        /// Adds the specified service.
        /// </summary>
        /// <param name="service">The service.</param>
        public Guid Add(IBackgroundProcess service)
        {
            var id = Process.Add(service);

            _serivceIds.Add(id);

            return id;
        }

        /// <summary>
        /// Adds the specified service.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <returns></returns>
        public async Task<Guid> AddAsync(IBackgroundProcess service)
        {
            return await Task.Run(() => Add(service));
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public void Stop()
        {
            var tasks = new List<Task>();

            foreach (var id in _serivceIds)
            {
                tasks.Add(Process.StopAsync(id));
            }

            foreach (var task in tasks)
            {
                task.Wait();
            }
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public async Task StopAsync()
        {
           await Task.Run(() => Stop());
        }

        /// <summary>
        /// 傳回會逐一查看集合的列舉程式。
        /// </summary>
        /// <returns>
        ///   <see cref="T:System.Collections.IEnumerator" /> 物件，用於逐一查看集合。
        /// </returns>
        public IEnumerator GetEnumerator()
        {
            return _serivceIds.GetEnumerator();
        }
    }
}