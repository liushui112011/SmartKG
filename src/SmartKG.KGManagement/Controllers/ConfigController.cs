﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog;
using SmartKG.Common.Data.Visulization;
using SmartKG.KGManagement.Data.Response;
using SmartKG.KGManagement.GraphSearch;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SmartKG.KGManagement.Controllers
{
    [Produces("application/json")]
    [ApiController]
    public class ConfigController : Controller
    {
        private ILogger log;

        public ConfigController()
        {
            log = Log.Logger.ForContext<ConfigController>();
        }

        [HttpGet]
        [Route("api/[controller]/entitycolor")]
        [ProducesResponseType(typeof(ConfigResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IResult>> Get(string datastoreName, string scenarioName)
        {
            ConfigResult result = new ConfigResult();

            if (string.IsNullOrWhiteSpace(datastoreName))
            {
                result.success = false;
                result.responseMessage = "datastoreName不能为空。";
            }
            else
            { 
                GraphExecutor executor = new GraphExecutor(datastoreName);

                (bool isDSExist, bool isScenarioExist, List<ColorConfig> configs) = executor.GetColorConfigs(scenarioName);

                if (!isDSExist)
                {
                    result.success = false;
                    result.responseMessage = "Datastore " + datastoreName + "不存在，或没有数据导入。";
                }
                else
                { 
                    if (!isScenarioExist)
                    {
                        result.success = false;
                        result.responseMessage = "scenario " + scenarioName + " 不存在。";
                    }
                    else
                    { 
                        result.success = true;

                        if (configs == null)
                        {
                            result.responseMessage = "scenario " + scenarioName + " 没有 color config 的定义。";
                        }
                        else
                        {
                            result.responseMessage = "一共有 " + configs.Count + " color config 的定义。";
                            result.entityColorConfig = new Dictionary<string,string>();

                            foreach(ColorConfig config in configs)
                            {
                                if (!result.entityColorConfig.ContainsKey(config.itemLabel))
                                { 
                                    result.entityColorConfig.Add(config.itemLabel, config.color);
                                }
                            }

                        }
                    }
                }
            }
            log.Information("[Response]: " + JsonConvert.SerializeObject(result));

            return Ok(result);
        }
    }
}
