resource "azurerm_cdn_frontdoor_profile" "waf" {
  count = local.waf_application == "CDN" ? 1 : 0

  name                     = "${local.resource_prefix}-cdnwaf"
  resource_group_name      = local.resource_group.name
  sku_name                 = local.cdn_sku
  response_timeout_seconds = local.response_request_timeout
  tags                     = local.tags
}

resource "azurerm_cdn_frontdoor_origin_group" "waf" {
  for_each = local.waf_application == "CDN" ? local.waf_targets : {}

  name                     = "${local.resource_prefix}-${each.key}"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.waf[0].id

  load_balancing {}

  dynamic "health_probe" {
    for_each = lookup(each.value, "enable_health_probe", true) ? [1] : []

    content {
      protocol            = "Https"
      interval_in_seconds = lookup(each.value, "health_probe_interval", 60)
      request_type        = lookup(each.value, "health_probe_request_type", "HEAD")
      path                = lookup(each.value, "health_probe_path", "/")
    }
  }
}

resource "azurerm_cdn_frontdoor_origin" "waf" {
  for_each = local.waf_application == "CDN" ? local.waf_targets : {}

  name                           = "${local.resource_prefix}origin-${each.key}"
  cdn_frontdoor_origin_group_id  = azurerm_cdn_frontdoor_origin_group.waf[each.key].id
  enabled                        = true
  certificate_name_check_enabled = true
  host_name                      = each.value.domain
  origin_host_header             = each.value.domain
  http_port                      = 80
  https_port                     = 443

  dynamic "private_link" {
    for_each = lookup(each.value, "create_private", false) ? [1] : []

    content {
      private_link_target_id = each.value.private_link_target_id
      location               = each.value.private_link_location
      target_type            = lookup(each.value, "private_link_target_type", "managedEnvironments")
    }
  }
}

resource "azurerm_cdn_frontdoor_endpoint" "waf" {
  for_each = local.waf_application == "CDN" ? local.waf_targets : {}

  name                     = substr("${local.resource_prefix}-${each.key}", 0, 46)
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.waf[0].id

  tags = local.tags
}

resource "azurerm_cdn_frontdoor_custom_domain" "waf" {
  for_each = local.waf_application == "CDN" ? local.cdn_custom_domains : {}

  name                     = "${local.resource_prefix}-${each.key}"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.waf[0].id
  host_name                = each.value

  tls {
    certificate_type    = "ManagedCertificate"
    minimum_tls_version = "TLS12"
  }
}

resource "azurerm_cdn_frontdoor_route" "waf" {
  for_each = local.waf_application == "CDN" ? local.waf_targets : {}

  name                          = "${local.resource_prefix}-${each.key}"
  cdn_frontdoor_endpoint_id     = azurerm_cdn_frontdoor_endpoint.waf[each.key].id
  cdn_frontdoor_origin_group_id = azurerm_cdn_frontdoor_origin_group.waf[each.key].id
  cdn_frontdoor_origin_ids = [
    azurerm_cdn_frontdoor_origin.waf[each.key].id
  ]
  cdn_frontdoor_rule_set_ids = flatten([
    lookup(azurerm_cdn_frontdoor_rule_set.origin_headers, each.key, "") != "" ? [azurerm_cdn_frontdoor_rule_set.origin_headers[each.key].id] : [],
    [azurerm_cdn_frontdoor_rule_set.global_headers[0].id],
    length(local.cdn_host_redirects) > 0 ? [azurerm_cdn_frontdoor_rule_set.redirects[0].id] : [],
    length(local.cdn_url_path_redirects) > 0 ? [azurerm_cdn_frontdoor_rule_set.url_path_redirects[0].id] : [],
  ])

  enabled                = true
  forwarding_protocol    = "HttpsOnly"
  https_redirect_enabled = true
  patterns_to_match      = ["/*"]
  supported_protocols    = ["Http", "Https"]

  cdn_frontdoor_custom_domain_ids = lookup(local.cdn_custom_domains, each.key, "") != "" ? [azurerm_cdn_frontdoor_custom_domain.waf[each.key].id] : null

  link_to_default_domain = true
}

resource "azurerm_cdn_frontdoor_custom_domain_association" "waf" {
  for_each = local.waf_application == "CDN" ? local.cdn_custom_domains : {}

  cdn_frontdoor_custom_domain_id = azurerm_cdn_frontdoor_custom_domain.waf[each.key].id
  cdn_frontdoor_route_ids        = [azurerm_cdn_frontdoor_route.waf[each.key].id]
}

resource "azurerm_cdn_frontdoor_rule_set" "redirects" {
  count = local.waf_application == "CDN" && length(local.cdn_host_redirects) > 0 ? 1 : 0

  name                     = "${replace(local.resource_prefix, "-", "")}redirects"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.waf[0].id
}

resource "azurerm_cdn_frontdoor_rule_set" "url_path_redirects" {
  count = local.waf_application == "CDN" && length(local.cdn_url_path_redirects) > 0 ? 1 : 0

  name                     = "${replace(local.resource_prefix, "-", "")}urlpathredirects"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.waf[0].id
}

resource "azurerm_cdn_frontdoor_rule" "redirect" {
  for_each = local.waf_application == "CDN" ? { for index, host_redirect in local.cdn_host_redirects : index => { "from" : host_redirect.from, "to" : host_redirect.to } } : {}

  depends_on = [azurerm_cdn_frontdoor_origin_group.waf, azurerm_cdn_frontdoor_origin.waf]

  name                      = "redirect${each.key}"
  cdn_frontdoor_rule_set_id = azurerm_cdn_frontdoor_rule_set.redirects[0].id
  order                     = each.key
  behavior_on_match         = "Continue"

  actions {
    url_redirect_action {
      redirect_type        = "Moved"
      redirect_protocol    = "Https"
      destination_hostname = each.value.to
    }
  }

  conditions {
    host_name_condition {
      operator         = "Equal"
      negate_condition = false
      match_values     = [each.value.from]
      transforms       = ["Lowercase", "Trim"]
    }
  }
}

resource "azurerm_cdn_frontdoor_rule" "url_path_redirect" {
  for_each = local.waf_application == "CDN" ? {
    for index, host_redirect in local.cdn_url_path_redirects : index => {
      redirect_type        = host_redirect.redirect_type,
      redirect_protocol    = host_redirect.redirect_protocol
      destination_path     = host_redirect.destination_path
      destination_hostname = host_redirect.destination_hostname,
      destination_fragment = host_redirect.destination_fragment,
      query_string         = host_redirect.query_string,
      operator             = host_redirect.operator,
      match_values         = host_redirect.match_values,
      transforms           = host_redirect.transforms
    }
  } : {}

  depends_on = [azurerm_cdn_frontdoor_origin_group.waf, azurerm_cdn_frontdoor_origin.waf]

  name                      = "urlredirect${each.key}"
  cdn_frontdoor_rule_set_id = azurerm_cdn_frontdoor_rule_set.url_path_redirects[0].id
  order                     = each.key
  behavior_on_match         = "Continue"

  actions {
    url_redirect_action {
      redirect_type        = each.value.redirect_type
      redirect_protocol    = each.value.redirect_protocol
      destination_hostname = each.value.destination_hostname
      destination_path     = each.value.destination_path
      destination_fragment = each.value.destination_fragment
      query_string         = each.value.query_string
    }
  }

  conditions {
    url_path_condition {
      operator     = each.value.operator
      match_values = each.value.match_values
      transforms   = each.value.transforms
    }
  }
}

resource "azurerm_cdn_frontdoor_rule_set" "global_headers" {
  count = local.waf_application == "CDN" ? 1 : 0

  name                     = "${replace(local.resource_prefix, "-", "")}headers"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.waf[0].id
}

resource "azurerm_cdn_frontdoor_rule" "add_response_headers" {
  for_each = local.waf_application == "CDN" ? { for index, response_header in local.cdn_add_response_headers : index => { "name" : response_header.name, "value" : response_header.value } } : {}

  depends_on = [azurerm_cdn_frontdoor_origin_group.waf, azurerm_cdn_frontdoor_origin.waf]

  name                      = replace("addresponseheaders${each.key}", "-", "")
  cdn_frontdoor_rule_set_id = azurerm_cdn_frontdoor_rule_set.global_headers[0].id
  order                     = each.key
  behavior_on_match         = "Continue"

  actions {
    response_header_action {
      header_action = "Overwrite"
      header_name   = each.value.name
      value         = each.value.value
    }
  }
}

resource "azurerm_cdn_frontdoor_rule" "remove_response_header" {
  for_each = local.waf_application == "CDN" ? toset(local.cdn_remove_response_headers) : []

  depends_on = [azurerm_cdn_frontdoor_origin_group.waf, azurerm_cdn_frontdoor_origin.waf]

  name                      = replace("removeresponseheader${each.value}", "-", "")
  cdn_frontdoor_rule_set_id = azurerm_cdn_frontdoor_rule_set.global_headers[0].id
  order                     = index(local.cdn_remove_response_headers, each.value) + length(local.cdn_add_response_headers)
  behavior_on_match         = "Continue"

  actions {
    response_header_action {
      header_action = "Delete"
      header_name   = each.value
    }
  }
}

resource "azurerm_cdn_frontdoor_rule_set" "origin_headers" {
  for_each = local.waf_application == "CDN" ? local.waf_targets : {}

  name                     = "${replace(each.key, "-", "")}headers"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.waf[0].id
}

resource "azurerm_cdn_frontdoor_rule" "add_origin_response_headers" {
  for_each = local.waf_application == "CDN" ? { for waf_target_name, data in local.waf_targets :
    waf_target_name => lookup(data, "cdn_add_response_headers", []) if length(lookup(data, "cdn_add_response_headers", [])) > 0
  } : {}

  depends_on = [azurerm_cdn_frontdoor_origin_group.waf, azurerm_cdn_frontdoor_origin.waf]

  name                      = "addresponseheaders${replace(each.key, "-", "")}"
  cdn_frontdoor_rule_set_id = azurerm_cdn_frontdoor_rule_set.origin_headers[each.key].id
  order                     = index(keys(local.waf_targets), each.key)
  behavior_on_match         = "Continue"

  actions {
    dynamic "response_header_action" {
      for_each = { for index, header in each.value : header.name => header.value }

      content {
        header_action = "Overwrite"
        header_name   = response_header_action.key
        value         = response_header_action.value
      }
    }
  }
}

resource "azurerm_cdn_frontdoor_rule" "remove_origin_response_headers" {
  for_each = local.waf_application == "CDN" ? { for waf_target_name, data in local.waf_targets :
    waf_target_name => lookup(data, "cdn_remove_response_headers", []) if length(lookup(data, "cdn_remove_response_headers", [])) > 0
  } : {}

  depends_on = [azurerm_cdn_frontdoor_origin_group.waf, azurerm_cdn_frontdoor_origin.waf]

  name                      = "removeresponseheaders${replace(each.key, "-", "")}"
  cdn_frontdoor_rule_set_id = azurerm_cdn_frontdoor_rule_set.origin_headers[each.key].id
  order                     = index(keys(local.waf_targets), each.key) + length(local.waf_targets)
  behavior_on_match         = "Continue"

  actions {
    dynamic "response_header_action" {
      for_each = each.value

      content {
        header_action = "Delete"
        header_name   = response_header_action.value
      }
    }
  }
}

resource "azurerm_cdn_frontdoor_rule" "add_origin_request_headers" {
  for_each = local.waf_application == "CDN" ? { for waf_target_name, data in local.waf_targets :
    waf_target_name => lookup(data, "cdn_add_request_headers", []) if length(lookup(data, "cdn_add_request_headers", [])) > 0
  } : {}

  depends_on = [azurerm_cdn_frontdoor_origin_group.waf, azurerm_cdn_frontdoor_origin.waf]

  name                      = "addrequestheaders${replace(each.key, "-", "")}"
  cdn_frontdoor_rule_set_id = azurerm_cdn_frontdoor_rule_set.origin_headers[each.key].id
  order                     = index(keys(local.waf_targets), each.key)
  behavior_on_match         = "Continue"

  actions {
    dynamic "request_header_action" {
      for_each = { for index, header in each.value : header.name => header.value }

      content {
        header_action = "Overwrite"
        header_name   = request_header_action.key
        value         = request_header_action.value
      }
    }
  }
}

resource "azurerm_cdn_frontdoor_rule" "remove_origin_request_headers" {
  for_each = local.waf_application == "CDN" ? { for waf_target_name, data in local.waf_targets :
    waf_target_name => lookup(data, "cdn_remove_request_headers", []) if length(lookup(data, "cdn_remove_request_headers", [])) > 0
  } : {}

  depends_on = [azurerm_cdn_frontdoor_origin_group.waf, azurerm_cdn_frontdoor_origin.waf]

  name                      = "removerequestheaders${replace(each.key, "-", "")}"
  cdn_frontdoor_rule_set_id = azurerm_cdn_frontdoor_rule_set.origin_headers[each.key].id
  order                     = index(keys(local.waf_targets), each.key) + length(local.waf_targets)
  behavior_on_match         = "Continue"

  actions {
    dynamic "request_header_action" {
      for_each = each.value

      content {
        header_action = "Delete"
        header_name   = request_header_action.value
      }
    }
  }
}
