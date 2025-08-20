dqeServices.factory('navigationService', ['$location', 'stateService', function ($location, stateService) {
    return {
        getNavs: function (currentUser) {
            if (currentUser.isAuthenticated) {
                //co admin or district admin
                if (currentUser.role == 'A' || currentUser.role == 'D' || currentUser.role == '2' || currentUser.role == 'O') {
                    return [
                        {
                            title: 'Home',
                            url: '/home_estimates',
                            active: $location.url().startsWith('/home') ? 'active' : ''
                        },
                        //{
                        //    title: 'Profile',
                        //    url: '/profile_projects',
                        //    active: $location.url().startsWith('/profile') ? 'active' : ''
                        //},
                        {
                            title: 'Administration',
                            url: '/admin_security',
                            active: $location.url().startsWith('/admin') ? 'active' : ''
                        },
                        {
                            title: 'Basis of Estimates',
                            url: '/boe',
                            active: $location.url().startsWith('/boe') ? 'active' : ''
                        },
                        {
                            title: 'Master Pay Item List',
                            url: '/payitems',
                            active: $location.url().startsWith('/payitems') ? 'active' : ''
                        }
                    ];
                } else if (currentUser.role == 'P' || currentUser.role == 'T' || currentUser.role == '2' || currentUser.role == 'O') {
                    //pay item admin or cost-based template admin
                    return [
                        {
                            title: 'Administration',
                            url: (currentUser.role == 4)
                                ? '/admin_payitems_maintain'
                                : (currentUser.role == 5)
                                ? '/admin_costbasedtemplates'
                                : '',
                            active: $location.url().startsWith('/admin') ? 'active' : ''
                        },
                        {
                            title: 'Basis of Estimates',
                            url: '/boe',
                            active: $location.url().startsWith('/boe') ? 'active' : ''
                        },
                        {
                            title: 'Master Pay Item List',
                            url: '/payitems',
                            active: $location.url().startsWith('/payitems') ? 'active' : ''
                        }
                    ];
                } else if (currentUser.role == 'E' || currentUser.role == 'R') {
                    //estimators
                    return [
                        {
                            title: 'Home',
                            url: '/home_estimates',
                            active: $location.url().startsWith('/home') ? 'active' : ''
                        },
                        //{
                        //    title: 'Profile',
                        //    url: '/profile_projects',
                        //    active: $location.url().startsWith('/profile') ? 'active' : ''
                        //},
                        {
                            title: 'Basis of Estimates',
                            url: '/boe',
                            active: $location.url().startsWith('/boe') ? 'active' : ''
                        },
                        {
                            title: 'Master Pay Item List',
                            url: '/payitems',
                            active: $location.url().startsWith('/payitems') ? 'active' : ''
                        }
                    ];
                } else {
                    return [];
                }

            } else {
                return [
                    {
                        title: 'Home',
                        url: '/signin',
                        active: $location.url().startsWith('/signin') ? 'active' : ''
                    },
                    {
                        title: 'Basis of Estimates',
                        url: '/boe',
                        active: $location.url().startsWith('/boe') ? 'active' : ''
                    },
                    {
                        title: 'Master Pay Item List',
                        url: '/payitems',
                        active: $location.url().startsWith('/payitems') ? 'active' : ''
                    }
                ];
            }
        },
        getTopTabs: function (currentUser) {
            if (currentUser.isAuthenticated) {
                if ($location.url().startsWith('/home')) {
                    //co admin, district admin, or estimator
                    if (currentUser.role == 'A' || currentUser.role == 'D' || currentUser.role == 'E' || currentUser.role == '1' || currentUser.role == 'R'
                        || currentUser.role == 'C' || currentUser.role == '2' || currentUser.role == 'M' || currentUser.role == 'O') {
                        return [
                            {
                                title: 'My Estimates',
                                active: $location.url().startsWith('/home_estimates'),
                                url: '/home_estimates'
                            },
                            {
                                title: 'Project',
                                active: $location.url().startsWith('/home_project'),
                                url: '/home_project'
                            },
                            {
                                title: 'Proposal',
                                active: $location.url().startsWith('/home_proposal'),
                                url: '/home_proposal'
                            },
                            //{
                            //    title: 'Master Pay Item List',
                            //    active: $location.url().startsWith('/home_payitems'),
                            //    url: '/home_payitems'
                            //},
                            //{
                            //    title: 'Gaming',
                            //    active: $location.url().startsWith('/home_gaming'),
                            //    url: '/home_gaming'
                            //},
                            //{
                            //    title: 'Snapshots',
                            //    active: $location.url().startsWith('/home_snapshots'),
                            //    url: '/home_snapshots'
                            //},
                            {
                                title: 'Reports',
                                active: $location.url().startsWith('/home_reports'),
                                url: '/home_reports_proposal_summary'
                            }
                        ];
                    }
                }
                if ($location.url().startsWith('/profile')) {
                    if (currentUser.role == 'E') {
                        return [
                            {
                                title: 'My Projects',
                                active: $location.url().startsWith('/profile_edit'),
                                url: '/profile_projects'
                            },
                            {
                                title: 'Default Values',
                                active: $location.url().startsWith('/profile_defaultvalues'),
                                url: '/profile_defaultvalues'
                            }
                        ];
                    } else {
                        return [
                            {
                                title: 'My Projects',
                                active: $location.url().startsWith('/profile'),
                                url: '/profile_projects'
                            }
                        ];
                    }
                }
                if ($location.url().startsWith('/admin')) {
                    //co admin
                    if (currentUser.role == 'A') {
                        return [
                            {
                                title: 'Security',
                                active: $location.url().startsWith('/admin_security'),
                                url: '/admin_security'
                            },
                            {
                                title: 'Pay Item Configuration',
                                active: $location.url().startsWith('/admin_payitems'),
                                url: '/admin_payitems_maintain'
                            },
                            {
                                title: 'Cost-Based Templates',
                                active: $location.url().startsWith('/admin_costbasedtemplates'),
                                url: '/admin_costbasedtemplates'
                            },
                            {
                                title: 'Reference Links',
                                active: $location.url().startsWith('/admin_weblinks'),
                                url: '/admin_weblinks'
                            },
                            //{
                            //    title: 'Code Values',
                            //    active: $location.url().startsWith('/admin_codevalues'),
                            //    url: '/admin_codevalues'
                            //},
                            {
                                title: 'Default Values',
                                active: $location.url().startsWith('/admin_defaultvalues'),
                                url: '/admin_defaultvalues_market_areas'
                            },
                            {
                                title: 'Cost Groups',
                                active: $location.url().startsWith('/admin_costgroups'),
                                url: '/admin_costgroups'
                            }
                        ];
                    } else if (currentUser.role == 'D') {
                        //district admin
                        return [
                            {
                                title: 'Security',
                                active: $location.url().startsWith('/admin_security'),
                                url: '/admin_security'
                            },
                            //{
                            //    title: 'Default Values',
                            //    active: $location.url().startsWith('/admin_defaultvalues'),
                            //    url: '/admin_defaultvalues_pricing_parameters'
                            //}
                        ];
                    } else if (currentUser.role == 'P') {
                        //pay item admin
                        return [
                            {
                                title: 'Pay Item Configuration',
                                active: $location.url().startsWith('/admin_payitems'),
                                url: '/admin_payitems_maintain'
                            },
                            {
                                title: 'Reference Links',
                                active: $location.url().startsWith('/admin_weblinks'),
                                url: '/admin_weblinks'
                            }
                            //{
                            //    title: 'Code Values',
                            //    active: $location.url().startsWith('/admin_codevalues'),
                            //    url: '/admin_codevalues'
                            //}
                            
                        ];
                    } else if (currentUser.role == 'T') {
                        //cost-based template admin
                        return [
                            {
                                title: 'Cost-Based Templates',
                                active: $location.url().startsWith('/admin_costbasedtemplates'),
                                url: '/admin_costbasedtemplates'
                            }
                        ];
                    }
                }
            }
            return [];
        },
        getSubTabs: function (currentUser) {
            if (currentUser.isAuthenticated) {
                if ($location.url().startsWith('/admin_payitems')) {
                    return [
                        {
                            title: 'Maintain Pay Items',
                            active: $location.url().startsWith('/admin_payitems_maintain'),
                            url: '/admin_payitems_maintain'
                        },
                        //{
                        //    title: 'Update Factors',
                        //    active: $location.url().startsWith('/admin_payitems_factors'),
                        //    url: '/admin_payitems_factors'
                        //},
                        {
                            title: 'Copy Master File',
                            active: $location.url().startsWith('/admin_payitems_opencopy'),
                            url: '/admin_payitems_opencopy'
                        }
                    ];
                }
                if ($location.url().startsWith('/admin_defaultvalues')) {
                    return [
                        //{
                        //    title: 'Pricing Parameters',
                        //    active: $location.url().startsWith('/admin_defaultvalues_pricing_parameters'),
                        //    url: '/admin_defaultvalues_pricing_parameters'
                        //},
                        {
                            title: 'Market Areas',
                            active: $location.url().startsWith('/admin_defaultvalues_market_areas'),
                            url: '/admin_defaultvalues_market_areas'
                        },
                        {
                            title: 'General Parameters',
                            active: $location.url().startsWith('/admin_defaultvalues_general'),
                            url: '/admin_defaultvalues_general'
                        }
                    ];
                }
                if ($location.url().startsWith('/home_project')) {
                    return [
                        {
                            title: 'Project Detail',
                            active: $location.url() == '/home_project' || $location.url().startsWith('/home_project/'),
                            url: '/home_project'
                        },
                        {
                            title: 'Prices',
                            active: $location.url() == '/home_project_prices' || $location.url().startsWith('/home_project_prices/'),
                            url: '/home_project_prices'
                        }
                    ];
                }
                if ($location.url().startsWith('/home_proposal')) {
                    return [
                        {
                            title: 'Proposal Detail',
                            active: $location.url() == '/home_proposal' || $location.url().startsWith('/home_proposal/'),
                            url: '/home_proposal'
                        },
                        {
                            title: 'Prices',
                            active: $location.url() == '/home_proposal_prices' || $location.url().startsWith('/home_proposal_prices/'),
                            url: '/home_proposal_prices'
                        }
                    ];
                }
                if ($location.url().startsWith('/home_workingestimate')) {
                    return [
                        {
                            title: 'Estimate',
                            active: $location.url().startsWith('/home_workingestimate_estimate'),
                            url: '/home_workingestimate_estimate'
                        },
                        {
                            title: 'LS/DB',
                            active: $location.url().startsWith('/home_workingestimate_lsdb'),
                            url: '/home_workingestimate_lsdb'
                        }
                    ];
                }
                if ($location.url().startsWith('/home_reports')) {
                    return [
                        {
                            title: 'Proposal Estimate',
                            active: $location.url().startsWith('/home_reports_proposal_summary'),
                            url: '/home_reports_proposal_summary'
                        },
                        {
                            title: 'Unbalanced Items',
                            active: $location.url().startsWith('/home_reports_unbalanced_items'),
                            url: '/home_reports_unbalanced_items'
                        },
                        {
                            title: 'Executive Summary',
                            active: $location.url().startsWith('/home_reports_summary_letting'),
                            url: '/home_reports_summary_letting'
                        },
                        {
                            title: 'Estimate Tolerances',
                            active: $location.url().startsWith('/home_reports_bid_tolerance'),
                            url: '/home_reports_bid_tolerance'
                        }
                    ];
                }
            }
            return [];
        }
    };
}]);