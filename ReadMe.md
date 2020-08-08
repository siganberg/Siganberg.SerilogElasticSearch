### Sample appsettings.json

```
{
    "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning",
        "System.Net.Http.HttpClient.Default.LogicalHandler": "Information"
      }
    },
    "WriteTo": {
      "Kibana-Sink": {
        "Name": "Console",
        "Args": {
          "formatter": "Siganberg.SirilogElasticSearch.Formatter.KibanaFormatter, Siganberg.SirilogElasticSearch"
        }
      }
    },
    "Enrich": [
      "FromLogContext"
    ]
  }
}
```

### Notes: 

*Use `System.Net.Http.HttpClient.Default.LogicalHandler` to override HttpClient downstream call logs*. 

*To use simple formatting on development machine (human readable logs), you can override the WriteTo nodes for example create appsettings.Development json white value like this*

```
{
    "Serilog": {
        "WriteTo": {
            "Kibana-Sink": "",
            "Console-Sink" : {
                "Name": "Console"
            }
        }
    }
}
```

