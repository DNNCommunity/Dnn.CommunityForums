const gulp = require('gulp');
const less = require('gulp-less');
const cleanCSS = require('gulp-clean-css');
const sourcemaps = require('gulp-sourcemaps');
const rename = require('gulp-rename');
const  merge = require('merge-stream');

const zip = require('gulp-zip');



// LESS ----------------------

const lessWatchPath = ['./_less/**/*.less'];

const cssCopyTo = "./";

function buildLess() { // Parse only the Skin.less file
  // 1. What less files to parse?
  var lessCss = gulp.src('./_less/theme.less')

    .pipe(sourcemaps.init())
    .pipe(less())
    .pipe(sourcemaps.write(cssCopyTo))
    .pipe(gulp.dest(cssCopyTo));


    var lessCssMin = gulp.src('./_less/theme.less')


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
  var srcPipe = gulp.src(['./**/*.*', "!./theme-source.zip.resources", "!./node_modules/**"])
    .pipe(zip('theme-source.zip.resources'))
    .pipe(gulp.dest('./'))

  cb();

}


// Watch task: watch LESS files for changes
// If any change, run LESS tasks
function watchTask() {

  gulp.watch(lessWatchPath, gulp.series(buildLess, packageSource));


}

exports.buildLess = buildLess;
exports.source = packageSource;

exports.default = watchTask;

