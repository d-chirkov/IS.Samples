﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IS.Models
{
    public class Client
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Secret { get; set; }
        public string Uri { get; set; }
    }
}