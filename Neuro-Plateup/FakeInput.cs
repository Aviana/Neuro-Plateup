using Controllers;
using System.Reflection;

namespace Neuro_Plateup
{
    public class FakeInput
    {
        private readonly MethodInfo triggerInputUpdateMethod;

        public FakeInput()
        {
            triggerInputUpdateMethod = typeof(BaseInputSource)
                .GetMethod("TriggerInputUpdate", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        public void Send(InputUpdateEvent evt)
        {
            triggerInputUpdateMethod.Invoke(InputSourceIdentifier.DefaultInputSource, new object[] { evt });
        }
    }
}