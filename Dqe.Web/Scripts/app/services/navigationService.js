dqeServices.factory('navigationService', ['$location', function ($location) {
    return {
        getNavs: function (currentUser) {
            if (currentUser.isAuthenticated) {
                //co admin or district admin
                if (currentUser.role == 2 || currentUser.role == 3) {
                    return [
                        {
                            title: 'Home',
                            url: '/home_project',
                            active: $location.url().startsWith('/home') ? 'active' : ''
                        },
                        {
                            title: 'Profile',
                            url: '/profile_projects',
                            active: $location.url().startsWith('/profile') ? 'active' : ''
                        },
                        {
                            title: 'Administration',
                            url: '/admin_security',
                            active: $location.url().startsWith('/admin') ? 'active' : ''
                        }
                    ];
                } else if (currentUser.role == 4 || currentUser.role == 5) {
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
                        }
                    ];
                } else if (currentUser.role == 6) {
                    //estimators
                    return [
                        {
                            title: 'Home',
                            url: '/home_project',
                            active: $location.url().startsWith('/home') ? 'active' : ''
                        },
                        {
                            title: 'Profile',
                            url: '/profile_projects',
                            active: $location.url().startsWith('/profile') ? 'active' : ''
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
                        active: $location.url().startsWith('/home') ? 'active' : ''
                    }
                ];
            }
        },
        getTopTabs: function (currentUser) {
            if (currentUser.isAuthenticated) {
                if ($location.url().startsWith('/home')) {
                    //co admin, district admin, or estimator
                    if (currentUser.role == 2 || currentUser.role == 3 || currentUser.role == 6) {
                        return [
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
                            {
                                title: 'Gaming',
                                active: $location.url().startsWith('/home_gaming'),
                                url: '/home_gaming'
                            },
                            {
                                title: 'Snapshots',
                                active: $location.url().startsWith('/home_snapshots'),
                                url: '/home_snapshots'
                            },
                            {
                                title: 'Reports',
                                active: $location.url().startsWith('/home_reports'),
                                url: '/home_reports'
                            }
                        ];
                    }
                }
                if ($location.url().startsWith('/profile')) {
                    if (currentUser.role == 6) {
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
                    if (currentUser.role == 2) {
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
                                title: 'Code Values',
                                active: $location.url().startsWith('/admin_codevalues'),
                                url: '/admin_codevalues'
                            },
                            {
                                title: 'Web Links',
                                active: $location.url().startsWith('/admin_weblinks'),
                                url: '/admin_weblinks'
                            },
                            {
                                title: 'Default Values',
                                active: $location.url().startsWith('/admin_defaultvalues'),
                                url: '/admin_defaultvalues_pricing_parameters'
                            }
                        ];
                    } else if (currentUser.role == 3) {
                        //district admin
                        return [
                            {
                                title: 'Security',
                                active: $location.url().startsWith('/admin_security'),
                                url: '/admin_security'
                            },
                            {
                                title: 'Default Values',
                                active: $location.url().startsWith('/admin_defaultvalues'),
                                url: '/admin_defaultvalues_pricing_parameters'
                            }
                        ];
                    } else if (currentUser.role == 4) {
                        //pay item admin
                        return [
                            {
                                title: 'Pay Item Configuration',
                                active: $location.url().startsWith('/admin_payitems'),
                                url: '/admin_payitems_maintain'
                            },
                            {
                                title: 'Code Values',
                                active: $location.url().startsWith('/admin_codevalues'),
                                url: '/admin_codevalues'
                            },
                            {
                                title: 'Web Links',
                                active: $location.url().startsWith('/admin_weblinks'),
                                url: '/admin_weblinks'
                            }
                        ];
                    } else if (currentUser.role == 5) {
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
                        {
                            title: 'Update Factors',
                            active: $location.url().startsWith('/admin_payitems_factors'),
                            url: '/admin_payitems_factors'
                        },
                        {
                            title: 'Open/Copy Master File',
                            active: $location.url().startsWith('/admin_payitems_opencopy'),
                            url: '/admin_payitems_opencopy'
                        }
                    ];
                }
                if ($location.url().startsWith('/admin_defaultvalues')) {
                    return [
                        {
                            title: 'Pricing Parameters',
                            active: $location.url().startsWith('/admin_defaultvalues_pricing_parameters'),
                            url: '/admin_defaultvalues_pricing_parameters'
                        },
                        {
                            title: 'Market Areas',
                            active: $location.url().startsWith('/admin_defaultvalues_market_areas'),
                            url: '/admin_defaultvalues_market_areas'
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
                            active: $location.url() == '/home_project_prices',
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
                            active: $location.url() == '/home_proposal_prices',
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
            }
            return [];
        }
    };
}]);