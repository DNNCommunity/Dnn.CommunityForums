import gulp from 'gulp';
import changed from 'gulp-changed';
import newer from 'gulp-newer';
import less from 'gulp-less';
import cleanCSS from 'gulp-clean-css';
import sourcemaps from 'gulp-sourcemaps';
import rename from 'gulp-rename';
import merge from 'merge-stream';
import zipPlugin from 'gulp-zip';

// LESS ----------------------

const lessWatchPath = ['./_src/less/**/*.less'];

const cssCopyTo = "./";

function buildLess() { // Parse only the Skin.less file
  // 1. What less files to parse?
  const lessCss = gulp.src('./_src/less/theme.less')
    .pipe(sourcemaps.init())
    .pipe(less())
    .pipe(sourcemaps.write(cssCopyTo))
    .pipe(gulp.dest(cssCopyTo));

  const lessCssMin = gulp.src('./_src/less/theme.less')
    .pipe(less())
    .pipe(rename({ suffix: '.min' }))
    .pipe(sourcemaps.init())
    .pipe(sourcemaps.write(cssCopyTo))
    .pipe(cleanCSS({ inline: ['none'] }))
    .pipe(gulp.dest(cssCopyTo));

  return merge(lessCss, lessCssMin);
}

function allTasks() {
  buildLess();
}

function packageSource(cb) {
  gulp.src(['./**/*.*', "!./theme-source.zip.resources", "!./node_modules/**"])
    .pipe(changed('./theme-source.zip.resources'))
    .pipe(zipPlugin('theme-source.zip.resources'))
    .pipe(gulp.dest('./'));

  cb();
}

// Watch task: watch LESS files for changes
// If any change, run LESS tasks
function watchTask() {
  gulp.watch(lessWatchPath, gulp.series(buildLess, packageSource));
}

export { buildLess, packageSource as source, watchTask as default };