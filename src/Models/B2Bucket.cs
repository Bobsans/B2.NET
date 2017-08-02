﻿using System.Collections.Generic;

namespace B2Net.Models {
	public class B2Bucket {
		public string BucketId { get; set; }
		public string BucketName { get; set; }
		public string BucketType { get; set; }
        public Dictionary<string,string> BucketInfo { get; set; }
        //public B2BucketLifecycleRules[] LifecycleRules { get; set; }
    }

    public class B2BucketLifecycleRules
    {
        public int DaysFromHidingToDeleting { get; set; }
        public int DaysFromUploadingToHiding { get; set; }
        public string FileNamePrefix { get; set; }
    }

	public class B2BucketListDeserializeModel {
		public List<B2Bucket> Buckets { get; set; }
	}
}
