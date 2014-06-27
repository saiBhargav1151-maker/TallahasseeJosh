dqeServices.factory('navigationService', [ '$location', function ($location) {
    return {
        getNavs: function () {
            return [
                {
                    title: 'Home',
                    url: '/home_selection_project',
                    active: $location.url().startsWith('/home') ? 'active' : ''
                },
                {
                    title: 'Profile',
                    url: '/profile',
                    active: $location.url().startsWith('/profile') ? 'active' : ''
                },
                {
                    title: 'Administration',
                    url: '/admin_security',
                    active: $location.url().startsWith('/admin') ? 'active' : ''
                }
            ];
        },
        getTopTabs: function () {
            if ($location.url().startsWith('/home')) {
                return [
                {
                    title: 'Project/Proposal',
                    active: $location.url().startsWith('/home_selection'),
                    url: '/home_selection_project'
                },
                {
                    title: 'Working Estimate',
                    active: $location.url().startsWith('/home_workingestimate'),
                    url: '/home_workingestimate_estimate'
                },
                {
                    title: 'Pricing',
                    active: $location.url().startsWith('/home_pricing'),
                    url: '/home_pricing_parameters'
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
                },
                {
                    title: 'Gaming',
                    active: $location.url().startsWith('/home_gaming'),
                    url: '/home_gaming'
                }
                ];
            }
            if ($location.url().startsWith('/admin')) {
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
                        title: 'Default Values',
                        active: $location.url().startsWith('/admin_defaultvalues'),
                        url: '/admin_defaultvalues'
                    }
                ];
            }
            return [];
        },
        getSubTabs: function () {
            if ($location.url().startsWith('/admin_payitems')) {
                return [
                    {
                        title: 'Maintain',
                        active: $location.url().startsWith('/admin_payitems_maintain'),
                        url: '/admin_payitems_maintain'
                    },
                    {
                        title: 'Open/Copy',
                        active: $location.url().startsWith('/admin_payitems_opencopy'),
                        url: '/admin_payitems_opencopy'
                    },
                    {
                        title: 'Structure',
                        active: $location.url().startsWith('/admin_payitems_structure'),
                        url: '/admin_payitems_structure'
                    },
                    {
                        title: 'Factors',
                        active: $location.url().startsWith('/admin_payitems_factors'),
                        url: '/admin_payitems_factors'
                    }
                ];
            }
            if ($location.url().startsWith('/home_pricing')) {
                return [
                    {
                        title: 'Parameters',
                        active: $location.url().startsWith('/home_pricing_parameters'),
                        url: '/home_pricing_parameters'
                    },
                    {
                        title: 'Prices',
                        active: $location.url().startsWith('/home_pricing_prices'),
                        url: '/home_pricing_prices'
                    }
                ];
            }
            if ($location.url().startsWith('/home_selection')) {
                return [
                    {
                        title: 'Project',
                        active: $location.url().startsWith('/home_selection_project'),
                        url: '/home_selection_project'
                    },
                    {
                        title: 'Proposal',
                        active: $location.url().startsWith('/home_selection_proposal'),
                        url: '/home_selection_proposal'
                    },
                    {
                        title: 'LRE',
                        active: $location.url().startsWith('/home_selection_lre'),
                        url: '/home_selection_lre'
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
            return [];
        }
    };
}]);