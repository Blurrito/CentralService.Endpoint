using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.Interfaces.Listeners
{
    public interface IListener
    {
        public void Start();
        public void Stop();
    }
}
