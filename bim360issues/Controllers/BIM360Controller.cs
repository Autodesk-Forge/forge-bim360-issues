/////////////////////////////////////////////////////////////////////
// Copyright (c) Autodesk, Inc. All rights reserved
// Written by Forge Partner Development
//
// Permission to use, copy, modify, and distribute this software in
// object code form for any purpose and without fee is hereby granted,
// provided that the above copyright notice appears in all copies and
// that both that copyright notice and the limited warranty and
// restricted rights notice below appear in all supporting
// documentation.
//
// AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS.
// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE.  AUTODESK, INC.
// DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
// UNINTERRUPTED OR ERROR FREE.
/////////////////////////////////////////////////////////////////////

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Autodesk.Forge;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Text;
using System.Net;

namespace bim360issues.Controllers
{
    public class BIM360Controller : ControllerBase
    {
        private const string BASE_URL = "https://developer.api.autodesk.com";

        [HttpGet]
        [Route("api/forge/bim360/container")]
        public async Task<dynamic> GetContainerAsync(string href)
        {
            string[] idParams = href.Split('/');
            string projectId = idParams[idParams.Length - 1];
            string hubId = idParams[idParams.Length - 3];

            Credentials credentials = await Credentials.FromSessionAsync(base.Request.Cookies, Response.Cookies);

            ProjectsApi projectsApi = new ProjectsApi();
            projectsApi.Configuration.AccessToken = credentials.TokenInternal;
            var project = await projectsApi.GetProjectAsync(hubId, projectId);
            var issues = project.data.relationships.issues.data;
            if (issues.type != "issueContainerId") return null;
            return new { ContainerId = issues["id"], HubId = hubId };
        }

        public async Task<IRestResponse> GetIssuesAsync(string containerId, string resource, string urn)
        {
            Credentials credentials = await Credentials.FromSessionAsync(base.Request.Cookies, Response.Cookies);
            urn = Encoding.UTF8.GetString(Convert.FromBase64String(urn));

            RestClient client = new RestClient(BASE_URL);
            RestRequest request = new RestRequest("/issues/v1/containers/{container_id}/{resource}", RestSharp.Method.GET);
            request.AddParameter("container_id", containerId, ParameterType.UrlSegment);
            request.AddParameter("filter[target_urn]", urn, ParameterType.QueryString);
            request.AddParameter("resource", resource, ParameterType.UrlSegment);
            request.AddHeader("Content-Type", "application/vnd.api+json");
            request.AddHeader("Authorization", "Bearer " + credentials.TokenInternal);
            return await client.ExecuteAsync(request);
        }

        public async Task<IRestResponse> GetUsers(string accountId)
        {
            TwoLeggedApi oauth = new TwoLeggedApi();
            dynamic bearer = await oauth.AuthenticateAsync(Credentials.GetAppSetting("FORGE_CLIENT_ID"), Credentials.GetAppSetting("FORGE_CLIENT_SECRET"), "client_credentials", new Scope[] { Scope.AccountRead });

            RestClient client = new RestClient(BASE_URL);
            RestRequest request = new RestRequest("/hq/v1/accounts/{account_id}/users", RestSharp.Method.GET);
            request.AddParameter("account_id", accountId.Replace("b.", string.Empty), ParameterType.UrlSegment);
            request.AddHeader("Authorization", "Bearer " + bearer.access_token);
            return await client.ExecuteAsync(request);
        }

        [HttpGet]
        [Route("api/forge/bim360/account/{accountId}/container/{containerId}/issues/{urn}")]
        public async Task<JArray> GetDocumentIssuesAsync(string accountId, string containerId, string urn)
        {
            IRestResponse documentIssuesResponse = await GetIssuesAsync(containerId, "quality-issues", urn);
            IRestResponse usersResponse = await GetUsers(accountId);

            dynamic issues = JObject.Parse(documentIssuesResponse.Content);
            dynamic users = JArray.Parse(usersResponse.Content);
            foreach (dynamic issue in issues.data)
            {
                issue.attributes.assigned_to_name = "Not yet assigned"; // default value?
                foreach (dynamic user in users)
                {
                    if (user.uid == issue.attributes.assigned_to)
                    {
                        issue.attributes.assigned_to_name = user.name;
                    }
                }
            }

            return issues.data;
        }

        public async Task<IRestResponse> PostIssuesAsync(string containerId, string resource, JObject data)
        {
            Credentials credentials = await Credentials.FromSessionAsync(base.Request.Cookies, Response.Cookies);

            RestClient client = new RestClient(BASE_URL);
            RestRequest request = new RestRequest("/issues/v1/containers/{container_id}/{resource}", RestSharp.Method.POST);
            request.AddParameter("container_id", containerId, ParameterType.UrlSegment);
            request.AddParameter("resource", resource, ParameterType.UrlSegment);
            request.AddHeader("Authorization", "Bearer " + credentials.TokenInternal);
            request.AddHeader("Content-Type", "application/vnd.api+json");
            request.AddParameter("text/json", Newtonsoft.Json.JsonConvert.SerializeObject(data), ParameterType.RequestBody);

            return await client.ExecuteAsync(request);
        }

        public async Task<IRestResponse> GetIssueTypesAsync(string containerId)
        {
            Credentials credentials = await Credentials.FromSessionAsync(base.Request.Cookies, Response.Cookies);

            RestClient client = new RestClient(BASE_URL);
            RestRequest request = new RestRequest("/issues/v1/containers/{container_id}/ng-issue-types?include=subtypes", RestSharp.Method.GET);
            request.AddParameter("container_id", containerId, ParameterType.UrlSegment);
            request.AddHeader("Authorization", "Bearer " + credentials.TokenInternal);
            request.AddHeader("Content-Type", "application/vnd.api+json");

            return await client.ExecuteAsync(request);
        }


        [HttpPost]
        [Route("api/forge/bim360/container/{containerId}/issues/{urn}")]
        public async Task<IActionResult> CreateDocumentIssuesAsync(string containerId, string urn, [FromBody]JObject data)
        {
            // for this sample, let's create Design issues
            // so we need the ngType and ngSubtype
            IRestResponse issueTypesResponse = await GetIssueTypesAsync(containerId);
            dynamic issueTypes = JObject.Parse(issueTypesResponse.Content);
            string ngTypeId = string.Empty;
            string ngSubtypeId = string.Empty;
            foreach (dynamic ngType in issueTypes.results)
            {
                if (ngType.title == "Design") // ngType we're looking for
                {
                    foreach (dynamic subType in ngType.subtypes)
                    {
                        if (subType.title == "Design") // ngSubtype we're looking for
                        {
                            ngSubtypeId = subType.id; break; // stop looping subtype...
                        }
                    }
                    ngTypeId = ngType.id; break; // stop looping type...
                }
            }
            // double check we got it
            if (string.IsNullOrWhiteSpace(ngTypeId) || string.IsNullOrWhiteSpace(ngSubtypeId)) return BadRequest();
            // and replace on the payload
            data["data"]["attributes"]["ng_issue_type_id"] = ngTypeId;
            data["data"]["attributes"]["ng_issue_subtype_id"] = ngSubtypeId;

            // now post to Quality-Issues
            IRestResponse documentIssuesResponse = await PostIssuesAsync(containerId, "quality-issues", data);

            return (documentIssuesResponse.StatusCode == HttpStatusCode.Created ? (IActionResult)Ok() : (IActionResult)BadRequest(documentIssuesResponse.Content));
        }
    }
}