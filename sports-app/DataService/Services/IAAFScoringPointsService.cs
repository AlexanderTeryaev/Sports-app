using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService.Services
{
    public class IAAFScoringPointsService
    {
        public static readonly string DefaultEdition = "y2017";
        public static readonly string OutdoorsVenue = "outdoor";
        public static readonly string IndoorsVenue = "indoor";
        public static readonly string FieldsVenue = "field";
        public static readonly string DecathlonVenue = "decathlon";

        private static readonly dynamic constants = new {
            y2017 = new {
                outdoor = new {
                    m = new {
                        //tracks
                        _100m = new { resultShift = -17, conversionFactor = 24.63, pointShift = 0 },
                        _200m = new { resultShift = -35.5, conversionFactor = 5.08, pointShift = 0 },
                        _300m = new { resultShift = -57.2, conversionFactor = 1.83, pointShift = 0 },
                        _400m = new { resultShift = -79, conversionFactor = 1.021, pointShift = 0 },
                        _600m = new { resultShift = -131, conversionFactor = 0.367, pointShift = 0 },
                        _800m = new { resultShift = -182, conversionFactor = 0.198, pointShift = 0 },
                        _1000m = new { resultShift = -240, conversionFactor = 0.1074, pointShift = 0 },
                        _1500m = new { resultShift = -385, conversionFactor = 0.04066, pointShift = 0 },
                        _1mile = new { resultShift = -415, conversionFactor = 0.0351, pointShift = 0 },
                        _2000m = new { resultShift = -528, conversionFactor = 0.02181, pointShift = 0 },
                        _3000m = new { resultShift = -840, conversionFactor = 0.00815, pointShift = 0 },
                        _2miles = new { resultShift = -904.8, conversionFactor = 0.00703, pointShift = 0 },
                        _5000m = new { resultShift = -1440, conversionFactor = 0.002778, pointShift = 0 },
                        _10000m = new { resultShift = -3150, conversionFactor = 0.000524, pointShift = 0 },
                        // hurdles
                        _110mh = new { resultShift = -25.8, conversionFactor = 7.66, pointShift = 0 },
                        _400mh = new { resultShift = -95.5, conversionFactor = 0.546, pointShift = 0 },
                        _2000mSt = new { resultShift = -660, conversionFactor = 0.01023, pointShift = 0 },
                        _3000mSt = new { resultShift = -1020, conversionFactor = 0.004316, pointShift = 0 },
                        // relays
                        _4x100m = new { resultShift = -69.5, conversionFactor = 1.236, pointShift = 0 },
                        _4x200m = new { resultShift = -144, conversionFactor = 0.29767, pointShift = 0 },
                        _4x400m = new { resultShift = -334, conversionFactor = 0.05026, pointShift = 0 },
                        // road running
                        _10km = new { resultShift = -3150, conversionFactor = 0.00052841, pointShift = 0 },
                        _15km = new { resultShift = -4868, conversionFactor = 0.0002162, pointShift = 0 },
                        _10miles = new { resultShift = -5250, conversionFactor = 0.0001852, pointShift = 0 },
                        _20km = new { resultShift = -6629, conversionFactor = 0.0001147, pointShift = 0 },
                        _half_marathon = new { resultShift = -7020, conversionFactor = 0.000102, pointShift = 0 },
                        _25km = new { resultShift = -8536, conversionFactor = 0.00006765, pointShift = 0 },
                        _30km = new { resultShift = -10531, conversionFactor = 0.00004353, pointShift = 0 },
                        _marathon = new { resultShift = -15600, conversionFactor = 0.0000191, pointShift = 0 },
                        _100km = new { resultShift = -48600, conversionFactor = 0.000001765, pointShift = 0 },
                        // race walking
                        _3kmW = new { resultShift = -1650, conversionFactor = 0.001209, pointShift = 0 },
                        _5kmW = new { resultShift = -2760, conversionFactor = 0.000436, pointShift = 0 },
                        _10kmW = new { resultShift = -5580, conversionFactor = 0.0001118, pointShift = 0 },
                        _20kmW = new { resultShift = -11400, conversionFactor = 0.00002735, pointShift = 0 },
                        _30kmW = new { resultShift = -19110, conversionFactor = 0.00000893, pointShift = 0 },
                        _35kmW = new { resultShift = -23400, conversionFactor = 0.00000576, pointShift = 0 },
                        _50kmW = new { resultShift = -37200, conversionFactor = 0.000002124, pointShift = 0 },
                        // field
                        _high_jump = new { resultShift = 11.534, conversionFactor = 32.29, pointShift = -5000 },
                        _pole_vault = new { resultShift = 39.39, conversionFactor = 3.042, pointShift = -5000 },
                        _long_jump = new { resultShift = 48.41, conversionFactor = 1.929, pointShift = -5000 },
                        _triple_jump = new { resultShift = 98.63, conversionFactor = 0.4611, pointShift = -5000 },
                        _shot_put = new { resultShift = 687.7, conversionFactor = 0.042172, pointShift = -20000 },
                        _discus_throw = new { resultShift = 2232.6, conversionFactor = 0.004007, pointShift = -20000 },
                        _hammer_throw = new { resultShift = 2669.4, conversionFactor = 0.0028038, pointShift = -20000 },
                        _javelin_throw = new { resultShift = 2886.8, conversionFactor = 0.0023974, pointShift = -20000 },
                        // combined
                        _decathlon = new { resultShift = 71170, conversionFactor = 0.00000097749, pointShift = -5000 },
                    },
                    f = new {
                        //tracks
                        _100m = new { resultShift = -22, conversionFactor = 9.92, pointShift = 0 },
                        _200m = new { resultShift = -45.5, conversionFactor = 2.242, pointShift = 0 },
                        _300m = new { resultShift = -77, conversionFactor = 0.7, pointShift = 0 },
                        _400m = new { resultShift = -110, conversionFactor = 0.335, pointShift = 0 },
                        _600m = new { resultShift = -184, conversionFactor = 0.1192, pointShift = 0 },
                        _800m = new { resultShift = -250, conversionFactor = 0.0688, pointShift = 0 },
                        _1000m = new { resultShift = -330, conversionFactor = 0.0382, pointShift = 0 },
                        _1500m = new { resultShift = -540, conversionFactor = 0.0134, pointShift = 0 },
                        _1mile = new { resultShift = -580, conversionFactor = 0.01165, pointShift = 0 },
                        _2000m = new { resultShift = -750, conversionFactor = 0.006766, pointShift = 0 },
                        _3000m = new { resultShift = -1200, conversionFactor = 0.002539, pointShift = 0 },
                        _2miles = new { resultShift = -1296.3, conversionFactor = 0.002157, pointShift = 0 },
                        _5000m = new { resultShift = -2100, conversionFactor = 0.000808, pointShift = 0 },
                        _10000m = new { resultShift = -4500, conversionFactor = 0.0001712, pointShift = 0 },
                        // hurdles
                        _100mh = new { resultShift = -30, conversionFactor = 3.98, pointShift = 0 },
                        _400mh = new { resultShift = -130, conversionFactor = 0.208567, pointShift = 0 },
                        _2000mSt = new { resultShift = -880, conversionFactor = 0.0045, pointShift = 0 },
                        _3000mSt = new { resultShift = -1510, conversionFactor = 0.001323, pointShift = 0 },
                        // relays
                        _4x100m = new { resultShift = -98, conversionFactor = 0.3895, pointShift = 0 },
                        _4x200m = new { resultShift = -212, conversionFactor = 0.0795, pointShift = 0 },
                        _4x400m = new { resultShift = -480, conversionFactor = 0.01562, pointShift = 0 },
                        // road running
                        _10km = new { resultShift = -4500, conversionFactor = 0.0001742, pointShift = 0 },
                        _15km = new { resultShift = -6905, conversionFactor = 0.0000732, pointShift = 0 },
                        _10miles = new { resultShift = -7438, conversionFactor = 0.000063, pointShift = 0 },
                        _20km = new { resultShift = -9357, conversionFactor = 0.0000396, pointShift = 0 },
                        _half_marathon = new { resultShift = -9900, conversionFactor = 0.0000353, pointShift = 0 },
                        _25km = new { resultShift = -12144, conversionFactor = 0.0000228, pointShift = 0 },
                        _30km = new { resultShift = -15123, conversionFactor = 0.00001426, pointShift = 0 },
                        _marathon = new { resultShift = -22800, conversionFactor = 0.00000595, pointShift = 0 },
                        _100km = new { resultShift = -61200, conversionFactor = 0.000000874, pointShift = 0 },
                        // race walking
                        _3kmW = new { resultShift = -1871, conversionFactor = 0.000881, pointShift = 0 },
                        _5kmW = new { resultShift = -3140, conversionFactor = 0.0003246, pointShift = 0 },
                        _10kmW = new { resultShift = -6437, conversionFactor = 0.0000779, pointShift = 0 },
                        _20kmW = new { resultShift = -13200, conversionFactor = 0.0000187, pointShift = 0 },
                        _30kmW = new { resultShift = -21545, conversionFactor = 0.0000069, pointShift = 0 },
                        _50kmW = new { resultShift = -39952, conversionFactor = 0.00000196, pointShift = 0 },
                        // field
                        _high_jump = new { resultShift = 10.574, conversionFactor = 39.34, pointShift = -5000 },
                        _pole_vault = new { resultShift = 34.83, conversionFactor = 3.953, pointShift = -5000 },
                        _long_jump = new { resultShift = 49.24, conversionFactor = 1.966, pointShift = -5000 },
                        _triple_jump = new { resultShift = 105.53, conversionFactor = 0.4282, pointShift = -5000 },
                        _shot_put = new { resultShift = 657.53, conversionFactor = 0.0462, pointShift = -20000 },
                        _discus_throw = new { resultShift = 2227.3, conversionFactor = 0.0040277, pointShift = -20000 },
                        _hammer_throw = new { resultShift = 2540, conversionFactor = 0.0030965, pointShift = -20000 },
                        _javelin_throw = new { resultShift = 2214.9, conversionFactor = 0.004073, pointShift = -20000 },
                        // combined
                        _heptathlon = new { resultShift = 55990, conversionFactor = 0.000001581, pointShift = -5000 },
                    }
                }, indoor = new {
                    m = new {
                        //tracks
                        _100m = new { resultShift = -17, conversionFactor = 24.63, pointShift = 0 },
                        _200m = new { resultShift = -35.5, conversionFactor = 5.08, pointShift = 0 },
                        _300m = new { resultShift = -57.2, conversionFactor = 1.83, pointShift = 0 },
                        _400m = new { resultShift = -79, conversionFactor = 1.021, pointShift = 0 },
                        _600m = new { resultShift = -131, conversionFactor = 0.367, pointShift = 0 },
                        _800m = new { resultShift = -182, conversionFactor = 0.198, pointShift = 0 },
                        _1000m = new { resultShift = -240, conversionFactor = 0.1074, pointShift = 0 },
                        _1500m = new { resultShift = -385, conversionFactor = 0.04066, pointShift = 0 },
                        _1mile = new { resultShift = -415, conversionFactor = 0.0351, pointShift = 0 },
                        _2000m = new { resultShift = -528, conversionFactor = 0.02181, pointShift = 0 },
                        _3000m = new { resultShift = -840, conversionFactor = 0.00815, pointShift = 0 },
                        _2miles = new { resultShift = -904.8, conversionFactor = 0.00703, pointShift = 0 },
                        _5000m = new { resultShift = -1440, conversionFactor = 0.002778, pointShift = 0 },
                        _10000m = new { resultShift = -3150, conversionFactor = 0.000524, pointShift = 0 },
                        // hurdles
                        _110mh = new { resultShift = -25.8, conversionFactor = 7.66, pointShift = 0 },
                        _400mh = new { resultShift = -95.5, conversionFactor = 0.546, pointShift = 0 },
                        _2000mSt = new { resultShift = -660, conversionFactor = 0.01023, pointShift = 0 },
                        _3000mSt = new { resultShift = -1020, conversionFactor = 0.004316, pointShift = 0 },
                        // relays
                        _4x100m = new { resultShift = -69.5, conversionFactor = 1.236, pointShift = 0 },
                        _4x200m = new { resultShift = -144, conversionFactor = 0.29767, pointShift = 0 },
                        _4x400m = new { resultShift = -334, conversionFactor = 0.05026, pointShift = 0 },
                        // road running
                        _10km = new { resultShift = -3150, conversionFactor = 0.00052841, pointShift = 0 },
                        _15km = new { resultShift = -4868, conversionFactor = 0.0002162, pointShift = 0 },
                        _10miles = new { resultShift = -5250, conversionFactor = 0.0001852, pointShift = 0 },
                        _20km = new { resultShift = -6629, conversionFactor = 0.0001147, pointShift = 0 },
                        _half_marathon = new { resultShift = -7020, conversionFactor = 0.000102, pointShift = 0 },
                        _25km = new { resultShift = -8536, conversionFactor = 0.00006765, pointShift = 0 },
                        _30km = new { resultShift = -10531, conversionFactor = 0.00004353, pointShift = 0 },
                        _marathon = new { resultShift = -15600, conversionFactor = 0.0000191, pointShift = 0 },
                        _100km = new { resultShift = -48600, conversionFactor = 0.000001765, pointShift = 0 },
                        // race walking
                        _3kmW = new { resultShift = -1650, conversionFactor = 0.001209, pointShift = 0 },
                        _5kmW = new { resultShift = -2760, conversionFactor = 0.000436, pointShift = 0 },
                        _10kmW = new { resultShift = -5580, conversionFactor = 0.0001118, pointShift = 0 },
                        _20kmW = new { resultShift = -11400, conversionFactor = 0.00002735, pointShift = 0 },
                        _30kmW = new { resultShift = -19110, conversionFactor = 0.00000893, pointShift = 0 },
                        _35kmW = new { resultShift = -23400, conversionFactor = 0.00000576, pointShift = 0 },
                        _50kmW = new { resultShift = -37200, conversionFactor = 0.000002124, pointShift = 0 },
                        // field
                        _high_jump = new { resultShift = 11.534, conversionFactor = 32.29, pointShift = -5000 },
                        _pole_vault = new { resultShift = 39.39, conversionFactor = 3.042, pointShift = -5000 },
                        _long_jump = new { resultShift = 48.41, conversionFactor = 1.929, pointShift = -5000 },
                        _triple_jump = new { resultShift = 98.63, conversionFactor = 0.4611, pointShift = -5000 },
                        _shot_put = new { resultShift = 687.7, conversionFactor = 0.042172, pointShift = -20000 },
                        _discus_throw = new { resultShift = 2232.6, conversionFactor = 0.004007, pointShift = -20000 },
                        _hammer_throw = new { resultShift = 2669.4, conversionFactor = 0.0028038, pointShift = -20000 },
                        _javelin_throw = new { resultShift = 2886.8, conversionFactor = 0.0023974, pointShift = -20000 },
                        // combined
                        _decathlon = new { resultShift = 71170, conversionFactor = 0.00000097749, pointShift = -5000 },
                    },
                    f = new {
                        //tracks
                        _100m = new { resultShift = -17, conversionFactor = 24.63, pointShift = 0 },
                        _200m = new { resultShift = -35.5, conversionFactor = 5.08, pointShift = 0 },
                        _300m = new { resultShift = -57.2, conversionFactor = 1.83, pointShift = 0 },
                        _400m = new { resultShift = -79, conversionFactor = 1.021, pointShift = 0 },
                        _600m = new { resultShift = -131, conversionFactor = 0.367, pointShift = 0 },
                        _800m = new { resultShift = -182, conversionFactor = 0.198, pointShift = 0 },
                        _1000m = new { resultShift = -240, conversionFactor = 0.1074, pointShift = 0 },
                        _1500m = new { resultShift = -385, conversionFactor = 0.04066, pointShift = 0 },
                        _1mile = new { resultShift = -415, conversionFactor = 0.0351, pointShift = 0 },
                        _2000m = new { resultShift = -528, conversionFactor = 0.02181, pointShift = 0 },
                        _3000m = new { resultShift = -840, conversionFactor = 0.00815, pointShift = 0 },
                        _2miles = new { resultShift = -904.8, conversionFactor = 0.00703, pointShift = 0 },
                        _5000m = new { resultShift = -1440, conversionFactor = 0.002778, pointShift = 0 },
                        _10000m = new { resultShift = -3150, conversionFactor = 0.000524, pointShift = 0 },
                        // hurdles
                        _110mh = new { resultShift = -25.8, conversionFactor = 7.66, pointShift = 0 },
                        _400mh = new { resultShift = -95.5, conversionFactor = 0.546, pointShift = 0 },
                        _2000mSt = new { resultShift = -660, conversionFactor = 0.01023, pointShift = 0 },
                        _3000mSt = new { resultShift = -1020, conversionFactor = 0.004316, pointShift = 0 },
                        // relays
                        _4x100m = new { resultShift = -69.5, conversionFactor = 1.236, pointShift = 0 },
                        _4x200m = new { resultShift = -144, conversionFactor = 0.29767, pointShift = 0 },
                        _4x400m = new { resultShift = -334, conversionFactor = 0.05026, pointShift = 0 },
                        // road running
                        _10km = new { resultShift = -3150, conversionFactor = 0.00052841, pointShift = 0 },
                        _15km = new { resultShift = -4868, conversionFactor = 0.0002162, pointShift = 0 },
                        _10miles = new { resultShift = -5250, conversionFactor = 0.0001852, pointShift = 0 },
                        _20km = new { resultShift = -6629, conversionFactor = 0.0001147, pointShift = 0 },
                        _half_marathon = new { resultShift = -7020, conversionFactor = 0.000102, pointShift = 0 },
                        _25km = new { resultShift = -8536, conversionFactor = 0.00006765, pointShift = 0 },
                        _30km = new { resultShift = -10531, conversionFactor = 0.00004353, pointShift = 0 },
                        _marathon = new { resultShift = -15600, conversionFactor = 0.0000191, pointShift = 0 },
                        _100km = new { resultShift = -48600, conversionFactor = 0.000001765, pointShift = 0 },
                        // race walking
                        _3kmW = new { resultShift = -1650, conversionFactor = 0.001209, pointShift = 0 },
                        _5kmW = new { resultShift = -2760, conversionFactor = 0.000436, pointShift = 0 },
                        _10kmW = new { resultShift = -5580, conversionFactor = 0.0001118, pointShift = 0 },
                        _20kmW = new { resultShift = -11400, conversionFactor = 0.00002735, pointShift = 0 },
                        _30kmW = new { resultShift = -19110, conversionFactor = 0.00000893, pointShift = 0 },
                        _35kmW = new { resultShift = -23400, conversionFactor = 0.00000576, pointShift = 0 },
                        _50kmW = new { resultShift = -37200, conversionFactor = 0.000002124, pointShift = 0 },
                        // field
                        _high_jump = new { resultShift = 11.534, conversionFactor = 32.29, pointShift = -5000 },
                        _pole_vault = new { resultShift = 39.39, conversionFactor = 3.042, pointShift = -5000 },
                        _long_jump = new { resultShift = 48.41, conversionFactor = 1.929, pointShift = -5000 },
                        _triple_jump = new { resultShift = 98.63, conversionFactor = 0.4611, pointShift = -5000 },
                        _shot_put = new { resultShift = 687.7, conversionFactor = 0.042172, pointShift = -20000 },
                        _discus_throw = new { resultShift = 2232.6, conversionFactor = 0.004007, pointShift = -20000 },
                        _hammer_throw = new { resultShift = 2669.4, conversionFactor = 0.0028038, pointShift = -20000 },
                        _javelin_throw = new { resultShift = 2886.8, conversionFactor = 0.0023974, pointShift = -20000 },
                        // combined
                        _decathlon = new { resultShift = 71170, conversionFactor = 0.00000097749, pointShift = -5000 },
                    }
                },
                field = new {
                    m = new {
                        _1900m_13_field = new[] {
                            314000,
                            315200,
                            316400,
                            317600,
                            318800,
                            320000,
                            321200,
                            322400,
                            323600,
                            324800,
                            326000,
                            327200,
                            328400,
                            329600,
                            330800,
                            332000,
                            333400,
                            334800,
                            336200,
                            337600,
                            339000,
                            340400,
                            341800,
                            343200,
                            344600,
                            346000,
                            347400,
                            348800,
                            350200,
                            351600,
                            353000,
                            354400,
                            355800,
                            357200,
                            358600,
                            360000,
                            361600,
                            363200,
                            364800,
                            366400,
                            368000,
                            369600,
                            371200,
                            372800,
                            374400,
                            376000,
                            377800,
                            379600,
                            381400,
                            383200,
                            385000,
                            387800,
                            389600,
                            391400,
                            393200,
                            394000,
                            396200,
                            398400,
                            400600,
                            402800,
                            405000,
                            407400,
                            409800,
                            412200,
                            414600,
                            417000,
                            420000,
                            423000,
                            426000,
                            429000,
                            432000,
                            435600,
                            439200,
                            442800,
                            446400,
                            450000,
                            457800,
                            466600,
                            472400,
                            480200,
                            489000,
                            498000,
                            507000,
                            516000,
                            525000,
                            534000,
                            543000,
                            552000,
                            561000,
                            570000,
                            580000,
                            590000,
                            600000,
                            610000,
                            620000,
                            630000,
                            640000,
                            650000,
                            660000,
                            670000,
                            680000
                        },
                        _2500m_field = new[] {
                            429000,
                            430600,
                            432200,
                            433800,
                            435400,
                            437000,
                            438600,
                            440200,
                            441800,
                            443400,
                            445000,
                            446800,
                            448600,
                            450400,
                            452200,
                            454000,
                            455800,
                            457600,
                            459400,
                            461200,
                            463000,
                            465000,
                            467000,
                            469000,
                            471000,
                            473000,
                            475000,
                            477000,
                            479000,
                            481000,
                            483000,
                            485000,
                            487000,
                            489000,
                            491000,
                            493000,
                            495200,
                            497400,
                            499600,
                            501800,
                            504000,
                            506200,
                            508400,
                            510600,
                            512800,
                            515000,
                            517600,
                            520200,
                            522800,
                            525400,
                            528000,
                            530600,
                            533200,
                            535800,
                            538400,
                            541000,
                            544200,
                            547400,
                            550600,
                            553800,
                            557000,
                            560600,
                            564200,
                            567800,
                            571400,
                            575000,
                            578800,
                            582600,
                            586400,
                            590200,
                            594000,
                            599200,
                            604400,
                            609600,
                            614800,
                            620000,
                            631000,
                            642000,
                            653000,
                            664000,
                            675000,
                            686000,
                            697000,
                            708000,
                            719000,
                            730000,
                            742000,
                            754000,
                            766000,
                            778000,
                            790000,
                            805000,
                            820000,
                            835000,
                            850000,
                            865000,
                            880000,
                            895000,
                            910000,
                            925000,
                            940000
                        },
                        _4500m_field = new[] {
                            813000,
                            816000,
                            819000,
                            822000,
                            825000,
                            828000,
                            831400,
                            834800,
                            838200,
                            841600,
                            845000,
                            848400,
                            851800,
                            855200,
                            858600,
                            862000,
                            865400,
                            868800,
                            872200,
                            875600,
                            879000,
                            882400,
                            885800,
                            889200,
                            892600,
                            896000,
                            899600,
                            903200,
                            906800,
                            910400,
                            914000,
                            918000,
                            922000,
                            926000,
                            930000,
                            934000,
                            938000,
                            942000,
                            946000,
                            950000,
                            954000,
                            958600,
                            963020,
                            967800,
                            972400,
                            977000,
                            981600,
                            986200,
                            990800,
                            995400,
                            1000000,
                            1005400,
                            1010800,
                            1016200,
                            1021600,
                            1027000,
                            1032400,
                            1037800,
                            1043200,
                            1048600,
                            1054000,
                            1060600,
                            1067200,
                            1073800,
                            1080040,
                            1087000,
                            1095800,
                            1102600,
                            1110400,
                            1118200,
                            1126000,
                            1135400,
                            1144800,
                            1154200,
                            1163600,
                            1174000,
                            1183400,
                            1192800,
                            1201200,
                            1210600,
                            1280000,
                            1290000,
                            1300000,
                            1310000,
                            1320000,
                            1330000,
                            1340000,
                            1350000,
                            1360000,
                            1370000,
                            1380000,
                            1390000,
                            1400000,
                            1410000,
                            1420000,
                            1430000,
                            1440000,
                            1450000,
                            1460000,
                            1470000,
                            1480000
                        }
                    },
                    f = new
                    {
                        _1100m_field = new[] {
                            211000,
                            212000,
                            213000,
                            214000,
                            215000,
                            216000,
                            217000,
                            218000,
                            219000,
                            220000,
                            221000,
                            222200,
                            223400,
                            224600,
                            225800,
                            227000,
                            228200,
                            229400,
                            230600,
                            231800,
                            233000,
                            234200,
                            235400,
                            236600,
                            237800,
                            239000,
                            240200,
                            241400,
                            242600,
                            243800,
                            245000,
                            246400,
                            247800,
                            249200,
                            250600,
                            252000,
                            253400,
                            254800,
                            256200,
                            257600,
                            259000,
                            260600,
                            262200,
                            263800,
                            265400,
                            267000,
                            268600,
                            270200,
                            271800,
                            273400,
                            275000,
                            276800,
                            278600,
                            280400,
                            282200,
                            284000,
                            285800,
                            287600,
                            289400,
                            291200,
                            293000,
                            295400,
                            297800,
                            300200,
                            302600,
                            305000,
                            307400,
                            309800,
                            312200,
                            314600,
                            317000,
                            320400,
                            323800,
                            327200,
                            330600,
                            334000,
                            340000,
                            347000,
                            355000,
                            364000,
                            375000,
                            386000,
                            397000,
                            408000,
                            419000,
                            430000,
                            442000,
                            454000,
                            466000,
                            478000,
                            490000,
                            505000,
                            520000,
                            535000,
                            550000,
                            565000,
                            580000,
                            595000,
                            610000,
                            625000,
                            640000
                        },
                        _1900m_15_field = new[] {
                            381000,
                            383000,
                            385000,
                            387000,
                            389000,
                            391000,
                            393200,
                            395400,
                            397600,
                            399800,
                            402000,
                            404200,
                            406400,
                            408600,
                            410800,
                            413000,
                            415200,
                            417400,
                            419600,
                            421800,
                            424000,
                            426200,
                            428400,
                            430600,
                            432800,
                            435000,
                            437600,
                            440200,
                            442800,
                            445400,
                            448000,
                            450600,
                            453200,
                            455800,
                            458400,
                            461000,
                            463800,
                            466600,
                            469400,
                            472200,
                            475000,
                            478000,
                            481000,
                            484000,
                            487000,
                            490000,
                            493200,
                            496400,
                            499600,
                            502800,
                            506000,
                            509400,
                            512800,
                            516200,
                            519600,
                            523000,
                            526800,
                            530600,
                            534400,
                            538200,
                            542000,
                            546400,
                            550800,
                            555200,
                            559600,
                            564000,
                            569000,
                            574000,
                            579000,
                            584000,
                            589000,
                            595000,
                            601000,
                            608000,
                            615000,
                            622000,
                            630000,
                            639000,
                            649000,
                            659000,
                            680000,
                            702000,
                            724000,
                            746000,
                            768000,
                            780000,
                            804000,
                            828000,
                            852000,
                            876000,
                            900000,
                            924000,
                            948000,
                            972000,
                            996000,
                            1020000,
                            1044000,
                            1068000,
                            1092000,
                            1116000,
                            1140000
                        },                      
                        _3100m_field = new[] {
                            676000,
                            679800,
                            683600,
                            687400,
                            691200,
                            695000,
                            699000,
                            703000,
                            707000,
                            711000,
                            715000,
                            719000,
                            723000,
                            727000,
                            731000,
                            735000,
                            739000,
                            743000,
                            747000,
                            751000,
                            755000,
                            759600,
                            764200,
                            768800,
                            773400,
                            778000,
                            782600,
                            787200,
                            791800,
                            796400,
                            801000,
                            805800,
                            810600,
                            815400,
                            820200,
                            825000,
                            830200,
                            835400,
                            840600,
                            845800,
                            851000,
                            856200,
                            861400,
                            866600,
                            871800,
                            877000,
                            883000,
                            889000,
                            895000,
                            901000,
                            907000,
                            913400,
                            919800,
                            926200,
                            932600,
                            939000,
                            946000,
                            953000,
                            960000,
                            967000,
                            974000,
                            981800,
                            989600,
                            997400,
                            1005200,
                            1013000,
                            1022600,
                            1032200,
                            1041800,
                            1051400,
                            1061000,
                            1073000,
                            1085000,
                            1097000,
                            1109000,
                            1121000,
                            1144000,
                            1169000,
                            1195000,
                            1225000,
                            1255000,
                            1285000,
                            1315000,
                            1345000,
                            1375000,
                            1405000,
                            1435000,
                            1465000,
                            1495000,
                            1585000,
                            1615000,
                            1645000,
                            1675000,
                            1705000,
                            1735000,
                            1765000,
                            1795000,
                            1825000,
                            1855000,
                            1885000,
                            1915000
                        }
                        
                    }
                }
            }
        };

        private static List<string> runTypes = new List<string>() { "_100m", "_200m", "_400m", "_800m", "_1000m", "_1500m", "_110mh", "_100mh", "_60m", "_60mh" };
        private static List<string> jumpTypes = new List<string>() { "_high_jump", "_pole_vault", "_long_jump", "_triple_jump"};


        


        private static readonly dynamic constantsCombined = new
        {
            y2017 = new
            {
                outdoor = new
                {
                    m = new
                    {
                        //tracks
                        _100m = new { a = 25.4347, b = 18.00, c = 1.81 },
                        _200m = new { a = 5.8425, b = 38.00, c = 1.81 },
                        _400m = new { a = 1.53775, b = 82.00, c = 1.81 },
                        _1500m = new { a = 0.03768, b = 480.00, c = 1.85 },
                        // hurdles
                        _110mh = new { a = 5.74352, b = 28.50, c = 1.92 },
                        // field
                        _high_jump = new { a = 0.8465, b = 75.00, c = 1.42 },
                        _pole_vault = new { a = 0.2797, b = 100.00, c = 1.35 },
                        _long_jump = new { a = 0.14354, b = 220.00, c = 1.40 },
                        _shot_put = new { a = 51.39, b = 1.50, c = 1.05 },
                        _discus_throw = new { a = 12.91, b = 4.00, c = 1.10 },
                        _javelin_throw = new { a = 10.14, b = 7.00, c = 1.08 }
                    },
                    f = new
                    {
                        //tracks
                        _200m = new { a = 4.99087, b = 42.50, c = 1.81 },
                        _800m = new { a = 0.11193, b = 254.00, c = 1.88 },
                        // hurdles
                        _100mh = new { a = 9.23076, b = 26.70, c = 1.835 },
                        // field
                        _high_jump = new { a = 1.84523, b = 75.00, c = 1.348 },
                        _long_jump = new { a = 0.188807, b = 210.00, c = 1.41 },
                        _shot_put = new { a = 56.0211, b = 1.50, c = 1.05 },
                        _javelin_throw = new { a = 15.9803, b = 3.80, c = 1.04 }
                    }
                },
                indoor = new
                {
                    m = new
                    {
                        //tracks
                        _60m = new { a = 58.0150, b = 11.50, c = 1.81 },
                        _1000m = new { a = 0.08713, b = 305.50, c = 1.85 },
                        _60mh = new { a = 20.5173, b = 15.50, c = 1.92 }
                    },
                    f = new
                    {
                        _60mh = new { a = 20.0479, b = 17.00, c = 1.835 }
                    }
                },
                decathlon = new
                {
                    m = new
                    {

                    },
                    f = new
                    {
                        //tracks
                        _100m = new { a = 17.8570, b = 21.0, c = 1.81 },
                        _400m = new { a = 1.34285, b = 91.7, c = 1.81 },
                        _1500m = new { a = 0.02883, b = 535, c = 1.88 },
                        _pole_vault = new { a = 0.44125, b = 100.00, c = 1.35 },
                        _discus_throw = new { a = 12.3311, b = 3.00, c = 1.10 }
                    }
                }
            }
        };

        // methods


        // by php code resul not correct. // test on 100m if you need to check out if its correct.
        public static int? getPoints(double? result, string edition, string venueType, int genderId, string disciplineType)
        {

            if (string.IsNullOrWhiteSpace(disciplineType))
            {
                return -2;
            }
            disciplineType = "_" + disciplineType;
            var gender = "m";
            if(genderId == 0)
            {
                gender = "f";
            }

            if (result == null)
			    return null;
            /*
            if (!$this->options['electronicMeasurement'])  //hand time corrections
		    {
                //For sprints & hurdles up to 200m
                if (in_array($this->options['discipline'], ['50m', '55m', '60m', '100m', '200m', '50mh', '55mh', '60mh', '100mh', '110mh']))
				$result += 0.24;

                //For sprints & hurdles up to 500m
                if (in_array($this->options['discipline'], ['300m', '400m', '500m', '400mh']))
				$result += 0.14;
            }
            */
            var firstLevel = constants.GetType().GetProperty(edition)?.GetValue(constants);
            var secondLevel = firstLevel.GetType().GetProperty(venueType)?.GetValue(firstLevel);
            var thirdLevel = secondLevel.GetType().GetProperty(gender)?.GetValue(secondLevel);
            

            if(thirdLevel == null)
            {
                return -1;
            }


            var fourthLevel = thirdLevel.GetType().GetProperty(disciplineType)?.GetValue(thirdLevel);
            if (fourthLevel == null)
            {
                return -4;
            }
            var points = 0.0;
            if (venueType == FieldsVenue)
            {
                var resultInMileSeconds = result*1000;              
                for (int i = 0; i < fourthLevel.Length; i++)
                {
                    var timeSpan = fourthLevel[i];
                    if (timeSpan >= resultInMileSeconds)
                    {
                        points = 1000 - i * 10;
                        break;
                    }
                }

            }
            else
            {

                var resultShift = fourthLevel.resultShift;
                var conversionFactor = fourthLevel.conversionFactor;
                var pointShift = fourthLevel.pointShift;

                var shiftedResult = result + resultShift;

                // for some (track) disciplines the resultShift is subtracting "0 points etalon" and shifted result above 0 means no points are awarded
                if (resultShift < 0 && shiftedResult >= 0)
                    return 0;

                points = (conversionFactor * shiftedResult * shiftedResult) + pointShift;
                if (points <= 0)
                    return 0;
            }
            return Convert.ToInt32(Math.Floor(points));
        }

        public static bool IsCompetitionDisciplineValid(string disciplineType, string venueType, int genderId, bool isMultiBattle) {
            disciplineType = "_" + disciplineType;
            var gender = "m";
            if (genderId == 0)
            {
                gender = "f";
            }
            if (isMultiBattle)
            {
                var firstLevel = constantsCombined.GetType().GetProperty(DefaultEdition).GetValue(constantsCombined);
                var secondLevel = firstLevel.GetType().GetProperty(venueType)?.GetValue(firstLevel);
                if (secondLevel == null)
                {
                    return false;
                }
                var thirdLevel = secondLevel.GetType().GetProperty(gender)?.GetValue(secondLevel);
                if (thirdLevel == null)
                {
                    return false;
                }
                var fourthLevel = thirdLevel.GetType().GetProperty(disciplineType)?.GetValue(thirdLevel);
                if (fourthLevel == null)
                {
                    return false;
                }
            }
            else
            {
                var firstLevel = constants.GetType().GetProperty(DefaultEdition).GetValue(constants);
                var secondLevel = firstLevel.GetType().GetProperty(venueType)?.GetValue(firstLevel);
                if (secondLevel == null)
                {
                    return false;
                }
                var thirdLevel = secondLevel.GetType().GetProperty(gender)?.GetValue(secondLevel);
                if (thirdLevel == null)
                {
                    return false;
                }
                var fourthLevel = thirdLevel.GetType().GetProperty(disciplineType)?.GetValue(thirdLevel);
                if (fourthLevel == null)
                {
                    return false;
                }
            }
            return true;
        }



        public static int? getPointsCombined(double? result, string edition, string venueType, int genderId, string disciplineType)
        {
            disciplineType = "_" + disciplineType;
            var gender = "m";
            if (genderId == 0)
            {
                gender = "f";
            }

            if (result == null)
                return null;

            var firstLevel = constantsCombined.GetType().GetProperty(edition).GetValue(constantsCombined);
            var secondLevel = firstLevel.GetType().GetProperty(venueType).GetValue(firstLevel);
            var thirdLevel = secondLevel.GetType().GetProperty(gender).GetValue(secondLevel);
            if (thirdLevel == null)
            {
                return null;
            }
            var fourthLevel = thirdLevel.GetType().GetProperty(disciplineType)?.GetValue(thirdLevel);
            if (fourthLevel== null)
            {
                return -4;
            }

            var a = fourthLevel.a;
            var b = fourthLevel.b;
            var c = fourthLevel.c;

            if (jumpTypes.Contains(disciplineType))
            {
                result = result*100; // in pdf says it should be converted to centimeters
            }

            var shiftedResult = result - b; 
            if (runTypes.Contains(disciplineType))
            {
                shiftedResult = b - result;
            }
            var points = a*Math.Pow(shiftedResult,c);
            return Convert.ToInt32(Math.Floor(points));
        }

        public static double GetResultForCalculation(long? result, int? format) {
            if (!result.HasValue)
            {
                return -1;
            }
            switch (format)
            {
                //Timer-Based
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 9:
                    {
                        return ((double)result) / 1000;
                    }
                //Length-Based
                case 10:
                case 6:
                case 11:
                case 7:
                case 8:
                    {
                        return ((double)result) / 1000;
                    }
                default:
                    {
                        return ((double)result) / 1000;
                    }
            }
        }
        // can be used at Startup.cs Configuration to test before init.
        public static void runTests() {

            var points = getPoints(1063, IAAFScoringPointsService.DefaultEdition, IAAFScoringPointsService.OutdoorsVenue, 1, "decathlon"); // 100
            Console.WriteLine(points);
            System.Diagnostics.Debug.WriteLine("point: " + points);
        }
    }
}
