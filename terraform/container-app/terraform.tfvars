mssql_firewall_ipv4_allow_list = {
    Dev = {
        JAG = { start_ip_range = "86.27.237.79", end_ip_range = "86.27.237.79" }
        DFE_VPN_1 = { start_ip_range = "208.127.46.234", end_ip_range = "208.127.46.238" }
        DFE_VPN_2 = { start_ip_range = "208.127.46.242", end_ip_range = "208.127.46.254" }
        Drew_Home = { start_ip_range = "208.127.46.248" }
        Gilaine = { start_ip_range = "208.127.46.254" }
        Gilaine_Home = { start_ip_range = "208.127.46.250" }
        Gilaine_Home2 = { start_ip_range = "208.127.46.255" }
        Gilaine_Home3 = { start_ip_range = "208.127.46.249" }
        Gilaine_Work = { start_ip_range = "208.127.50.230" }
        Gilaine_Work2 = { start_ip_range = "208.127.45.99" }
        Gilaine_AZ_Data_Studio = { start_ip_range = "208.127.46.110" }
        Ken = { start_ip_range = "149.13.184.32" }
    },
    Test = {},
    Staging = {},
    Prod = {},
}

az_sql_sku = {
    Dev = "Basic",
    Test = "Basic",
    Staging = "S2",
    Prod = "S2"
}

cdn_create_custom_domain = {
    Dev = false,
    Test = false,
    Staging = true,
    Prod = true
}
