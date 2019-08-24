using G_Hoover.Models;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G_Hoover.Events
{
    public class UpdateStatusEvent : PubSubEvent<StatusPropertiesModel>
    {
    }
}
