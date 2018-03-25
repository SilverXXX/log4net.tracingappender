using log4net.Appender;
using log4net.Core;
using OpenTracing;
using OpenTracing.Util;
using System;
using System.ComponentModel;
using System.Threading;

namespace log4net_tracing_appender
{
    public class TracingAppender : ForwardingAppender
    {
        private void EnrichAndSendOne(object state)
        {
            LoggingEvent ev = (LoggingEvent)state;
            if (GlobalTracer.Instance.ActiveSpan != null)
                ev.Properties["opentracing.id"] = GlobalTracer.Instance.ActiveSpan.Context.ToString();

            base.Append(ev);
        }

        private void EnrichAndSendMany(object state)
        {
            LoggingEvent[] events = (LoggingEvent[])state;

            foreach (var item in events)
            {
                if (GlobalTracer.Instance.ActiveSpan != null)
                    item.Properties["opentracing.id"] = GlobalTracer.Instance.ActiveSpan.Context.ToString();
            }
            base.Append(events);
        }

        protected override void Append(LoggingEvent loggingEvent)
        {
            ThreadPool.QueueUserWorkItem(EnrichAndSendOne, loggingEvent);
        }

        protected override void Append(LoggingEvent[] loggingEvents)
        {
            ThreadPool.QueueUserWorkItem(EnrichAndSendMany, loggingEvents);
        }
    }
}
