using System;

namespace AI.Common.Tracking
{
	[Serializable]
	public class DigitalExhaust
	{
		public virtual long Id { get; set; }
		public virtual string UserName { get; set; }
		public virtual DateTime EventDateTime { get; set; }
		public virtual string Controller { get; set; }
		public virtual string ActionOrUrl { get; set; }
		public virtual string ControllerActionDetails { get; set; }
		public virtual string RouteData { get; set; }
		public virtual bool IsAuthenticated { get; set; }
		public virtual bool IsSecure { get; set; }
		public virtual bool IsAjax { get; set; }
		public virtual string QueryString { get; set; }
		public virtual string RefererUrl { get; set; }
		public virtual string UserAgent { get; set; }
		public virtual string RemoteAddress { get; set; }
		public virtual string ExtraData { get; set; }
		public virtual DateTime LoggedDateTime { get; set; }

        public override string ToString()
        {
            return String.Format("User:{0}, EventDateTime:{1}", UserName, EventDateTime.ToString("yyyyMMddHHmmssfff"));
        }
	}
}