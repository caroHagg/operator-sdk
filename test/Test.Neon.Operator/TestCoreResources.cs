// -----------------------------------------------------------------------------
// FILE:	    TestCoreResources.cs
// CONTRIBUTOR: NEONFORGE Team
// COPYRIGHT:   Copyright © 2005-2023 by NEONFORGE LLC.  All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License").
// You may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FluentAssertions;

using k8s;
using k8s.Models;

using Neon.Operator.Util;
using Neon.Operator.Xunit;
using Test.Neon.Operator;
using Xunit;

namespace TestKubeOperator
{
    public class TestCoreResources : IClassFixture<TestOperatorFixture>
    {
        private TestOperatorFixture fixture;
        public TestCoreResources(TestOperatorFixture fixture)
        {
            this.fixture = fixture;
            fixture.RegisterType<V1ConfigMap>();
            fixture.RegisterType<V1Service>();
            fixture.Start();
        }

        [Fact]
        public async Task TestGetConfigMapAsync()
        {
            fixture.ClearResources();

            var configMap = new V1ConfigMap()
            {
                Metadata = new V1ObjectMeta()
                {
                    Name = "test",
                    NamespaceProperty = "test",
                },
                Data = new Dictionary<string, string>(){ { "foo", "bar" } }
            };

            fixture.AddResource<V1ConfigMap>(configMap);

            var result = await fixture.KubernetesClient.CoreV1.ReadNamespacedConfigMapAsync(configMap.Metadata.Name, configMap.Metadata.NamespaceProperty);

            result.Should().NotBeNull();
        }
        [Fact]
        public async Task TestCreateConfigMapAsync()
        {
            fixture.ClearResources();

            var configMap = new V1ConfigMap()
            {
                Metadata = new V1ObjectMeta()
                {
                    Name = "test",
                    NamespaceProperty = "test",
                },
                Data = new Dictionary<string, string>(){ { "foo", "bar" } }
            };

            await fixture.KubernetesClient.CoreV1.CreateNamespacedConfigMapAsync(configMap, configMap.Metadata.Name, configMap.Metadata.NamespaceProperty);

            var created = fixture.GetResource<V1ConfigMap>(configMap.Metadata.Name,configMap.Metadata.NamespaceProperty);
            created.Should().NotBeNull();
            created.Data.Should().ContainKey("foo");

            fixture.Resources.Should().HaveCount(1);
        }

        [Fact]
        public async Task TestUpdateConfigMapAsync()
        {
            fixture.ClearResources();

            var configMap = new V1ConfigMap()
            {
                Metadata = new V1ObjectMeta()
                {
                    Name = "test",
                    NamespaceProperty = "test",
                },
                Data = new Dictionary<string, string>(){ { "foo", "bar" } }
            };

            fixture.AddResource<V1ConfigMap>(configMap);

            var config2 = new V1ConfigMap().Initialize();
            config2.Metadata.Name = "test-2";
            config2.Metadata.NamespaceProperty = "test";

            fixture.AddResource(config2);

            configMap.Data.Add("bar", "baz");

            await fixture.KubernetesClient.CoreV1.ReplaceNamespacedConfigMapAsync(configMap,configMap.Metadata.Name, configMap.Metadata.NamespaceProperty);

            var updated = fixture.GetResource<V1ConfigMap>(configMap.Metadata.Name,configMap.Metadata.NamespaceProperty);
            updated.Should().NotBeEquivalentTo(configMap);
            updated.Data.Should().ContainKey("foo");
            updated.Data.Should().ContainKey("bar");
            updated.Data["foo"].Should().Be("bar");
            updated.Data["bar"].Should().Be("baz");

            fixture.Resources.Should().HaveCount(2);
        }
        [Fact]
        public async Task TestPatchConfigMapAsync()
        {
            fixture.ClearResources();

            var configMap = new V1ConfigMap()
            {
                Metadata = new V1ObjectMeta()
                {
                    Name = "test",
                    NamespaceProperty = "test",
                },
                Data = new Dictionary<string, string>(){ { "foo", "bar" } }
            };

            fixture.AddResource<V1ConfigMap>(configMap);

            var config2 = new V1ConfigMap().Initialize();
            config2.Metadata.Name = "test-2";
            config2.Metadata.NamespaceProperty = "test";

            fixture.AddResource(config2);

            var patch = OperatorHelper.CreatePatch<V1ConfigMap>();

            patch.Add(path => path.Data["bar"], "baz");

            await fixture.KubernetesClient.CoreV1.PatchNamespacedConfigMapAsync(OperatorHelper.ToV1Patch<V1ConfigMap>(patch), configMap.Metadata.Name, configMap.Metadata.NamespaceProperty);

            var updated = fixture.GetResource<V1ConfigMap>(configMap.Metadata.Name,configMap.Metadata.NamespaceProperty);
            updated.Should().NotBeEquivalentTo(configMap);
            updated.Data.Should().ContainKey("foo");
            updated.Data.Should().ContainKey("bar");
            updated.Data["foo"].Should().Be("bar");
            updated.Data["bar"].Should().Be("baz");

            fixture.Resources.Should().HaveCount(2);
        }
        [Fact]
        public async Task TestDeleteConfigMapAsync()
        {
            fixture.ClearResources();

            var configMap = new V1ConfigMap()
            {
                Metadata = new V1ObjectMeta()
                {
                    Name = "test",
                    NamespaceProperty = "test",
                },
                Data = new Dictionary<string, string>(){ { "foo", "bar" } }
            };

            fixture.AddResource<V1ConfigMap>(configMap);

            var config2 = new V1ConfigMap().Initialize();
            config2.Metadata.Name = "test-2";
            config2.Metadata.NamespaceProperty = "test";

            fixture.AddResource(config2);

            await fixture.KubernetesClient.CoreV1.DeleteNamespacedConfigMapAsync(configMap.Metadata.Name, configMap.Metadata.NamespaceProperty);

            var deleted = fixture.GetResource<V1ConfigMap>(configMap.Metadata.Name,configMap.Metadata.NamespaceProperty);
            deleted.Should().BeNull();

            fixture.Resources.Should().HaveCount(1);
        }

        [Fact]
        public async Task TestGetListConfigMapAsync()
        {
            fixture.ClearResources();

            var configMap = new V1ConfigMap()
            {
                Metadata = new V1ObjectMeta()
                {
                    Name = "test",
                    NamespaceProperty = "test",
                },
                Data = new Dictionary<string, string>(){ { "foo", "bar" } }
            };

            await fixture.KubernetesClient.CoreV1.CreateNamespacedConfigMapAsync(configMap, configMap.Metadata.NamespaceProperty);

            var config2 = new V1ConfigMap().Initialize();
            config2.Metadata.Name = "test-2";
            config2.Metadata.NamespaceProperty = "test";

            await fixture.KubernetesClient.CoreV1.CreateNamespacedConfigMapAsync(config2, config2.Metadata.NamespaceProperty);

            var result = await fixture.KubernetesClient.CoreV1.ListNamespacedConfigMapAsync(configMap.Metadata.NamespaceProperty);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task TestGetListServiceAsync()
        {
            fixture.ClearResources();

            var service = new V1Service()
            {
                Metadata = new V1ObjectMeta()
                {
                    Name = "test",
                    NamespaceProperty = "test",
                },
                Spec = new V1ServiceSpec()
                {
                    Ports = new List<V1ServicePort>()
                    {
                        new V1ServicePort()
                        {
                            Name          = "http",
                            Protocol      = "TCP",
                            Port          = 6333,
                            TargetPort    = 6333
                        },
                        new V1ServicePort()
                        {
                            Name          = "grpc",
                            Protocol      = "TCP",
                            Port          = 6334,
                            TargetPort    = 6334
                        },
                        new V1ServicePort()
                        {
                            Name          = "p2p",
                            Protocol      = "TCP",
                            Port          = 6335,
                            TargetPort    = 6335
                        }
                    },
                    Selector              = new Dictionary<string, string>() {{"foo", "bar"}},
                    Type                  = "ClusterIP",
                    InternalTrafficPolicy = "Cluster"
                }
            };

            fixture.AddResource(service);

            var result = await fixture.KubernetesClient.CoreV1.ListNamespacedServiceAsync(service.Metadata.NamespaceProperty);

            result.Should().NotBeNull();
        }
    }
}
