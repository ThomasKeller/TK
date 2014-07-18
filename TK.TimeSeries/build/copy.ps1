robocopy $build_artifacts_release_dir $deploy_dir TK.Logger.* log4net.* /XF *.pdb
robocopy $build_artifacts_release_dir $deploy_dir TK.TimeSeries.* CompressionConfigs.xml MeasuredValueDB.sdf /XF *.pdb *.Test.*

robocopy $build_artifacts_debug_dir "$deploy_dir\Debug" TK.Logger.* log4net.* /XF *.pdb
robocopy $build_artifacts_debug_dir "$deploy_dir\Debug" TK.TimeSeries.* CompressionConfigs.xml MeasuredValueDB.sdf /XF *.Test.*