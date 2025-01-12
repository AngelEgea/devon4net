﻿using System.Collections;
using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.S3;

namespace Devon4Net.Infrastructure.AWS.CDK.Entities
{
    public class BucketEntity
    {
        public string BucketName { get; set; }
        public int ExpirationDays { get; set; }
        public RemovalPolicy RemovalPolicy { get; set; }
        public bool Versioned { get; set; }
        public string WebSiteRedirectHost { get; set; }
        public BucketEncryption Encryption { get; set; }
        public IList<ILifecycleRule> LifecycleRules { get; set; }
        public bool? EnforceSSL { get; set; }
        public bool BlockPublicAccess { get; set; }
    }
}
