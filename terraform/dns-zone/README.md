# DNS Zone Terraform

Similar to the container app terraform, this folder is used to standup the dns zones for each environment. For dev and test we will only suppy a `primary_fqdn` for setting up the single dns zone that we have in each of those subscriptions. As production has staging as a subdomain zone, we can pass that in as a `['staging']` so that it creates the zone and the associated NS record.

## Deploying a DNS zone

When deploying a new DNS zone there are a couple of steps to runt through:

1. Make sure a storage environment exists fo rthe backend. Ideally this should re-use the same storage as [container-app](../container-app/) but with a unique key for the statefile.
2. Apply the terraform in `dns-zone` while supplying the fully qualified domain name passed in as `primary_fqdn=plan-technology-for-your-school.education.gov.uk`
3. The terraform will output the name servers which can be submitted to InfraOps to get the domain delegated across here: [How To Request DNS](https://dfe-ssp.visualstudio.com/s190-schools-technology-services/_wiki/wikis/s190-schools-technology-services.wiki/12901/How-to-request-DNS)

   Example output:

   ```bash
   ...
   Apply complete! Resources: 3 added, 0 changed, 1 destroyed.

    Outputs:

    primary-zone = {
    "domain" = "plan-technology-for-your-school.education.gov.uk"
    "name_servers" = toset([
        "ns1-XX.azure-dns.com.",
        "ns2-XX.azure-dns.net.",
        "ns3-XX.azure-dns.org.",
        "ns4-XX.azure-dns.info.",
    ])
    }
    resource_group_name = "prod-dns-zone"
    subdomain-zones = []
   ```

## Deploying a DNS zone for Prod & Staging

This is a slight variation on the above, as in prod we deploy the staging DNS zone as a subdomain of the production DNS zone.

When deploying a new DNS zone there are a couple of steps to runt through:

1. Make sure a storage account exists for the backend. Ideally this should re-use the same storage as [cluster-app](../container-app/) but with a unique key for the statefile.
2. Apply the terraform in `dns-zone` while supplying the fully qualified domain name passed in as `primary_fqdn=plan-technology-for-your-school.education.gov.uk` and `subdomains='["staging"]'`. Note: we use a set here so that the code can support additional subdomains in the future.
3. The terraform will output the name servers which can be submitted to InfraOps to get the domain delegated across here: [How To Request DNS](https://dfe-ssp.visualstudio.com/s190-schools-technology-services/_wiki/wikis/s190-schools-technology-services.wiki/12901/How-to-request-DNS)

   Example output:

   ```bash
   ...
   Apply complete! Resources: 3 added, 0 changed, 1 destroyed.

    Outputs:

    primary-zone = {
    "domain" = "plan-technology-for-your-school.education.gov.uk"
    "name_servers" = toset([
        "ns1-XX.azure-dns.com.",
        "ns2-XX.azure-dns.net.",
        "ns3-XX.azure-dns.org.",
        "ns4-XX.azure-dns.info.",
    ])
    }
    resource_group_name = "prod-dns-zone"
    subdomain-zones = [
    {
        "domain" = "staging.plan-technology-for-your-school.education.gov.uk"
        "name_servers" = toset([
        "ns1-XX.azure-dns.com.",
        "ns2-XX.azure-dns.net.",
        "ns3-XX.azure-dns.org.",
        "ns4-XX.azure-dns.info.",
        ])
    },
    ]
   ```

## CDN/WAF Domain association

All current environments have had their front door CDN/WAF association setup manually. In the future, these could be setup by simpling setting the value of `cdn_create_custom_domain` to `true` in [container-app](../container-app)
