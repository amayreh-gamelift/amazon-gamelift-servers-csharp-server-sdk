/*
* All or portions of this file Copyright (c) Amazon.com, Inc. or its affiliates or
* its licensors.
*
* For complete copyright and license terms please see the LICENSE at the root of this
* distribution (the "License"). All use of this software is governed by the License,
* or, if provided, by the license below or the license accompanying this file. Do not
* remove or modify any license notices. This file is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
*
*/

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace Aws.GameLift.Server.Model
{
    /// <summary>
    /// Values for use in <seealso cref="Player"/> attribute key-value pairs. This object 
    /// lets you specify an attribute value using any of the valid data types: 
    /// string, number, string array, or data map. Each <c>AttributeValue</c> object 
    /// can use only one of the available properties.
    /// </summary>
    public class AttributeValue
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum AttrType
        {
            STRING,
            DOUBLE,
            STRING_LIST,
            STRING_DOUBLE_MAP
        }

        public AttrType attrType { get; private set; }
        public string S { get; private set; }
        public double N { get; private set; }
        public string[] SL { get; private set; }
        public Dictionary<string, double> SDM { get; private set; }

        public AttributeValue(string s)
        {
            this.attrType = AttrType.STRING;
            this.S = s;
        }

        public AttributeValue(double n)
        {
            this.attrType = AttrType.DOUBLE;
            this.N = n;
        }

        public AttributeValue(string[] sl)
        {
            this.attrType = AttrType.STRING_LIST;
            this.SL = sl;
        }

        public AttributeValue(Dictionary<string, double> sdm)
        {
            this.attrType = AttrType.STRING_DOUBLE_MAP;
            this.SDM = sdm;
        }
    }
}
