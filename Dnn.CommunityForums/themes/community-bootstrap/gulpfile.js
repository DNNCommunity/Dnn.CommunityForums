const gulp = require('gulp');
const sass = require('gulp-sass')(require('sass'));
const cleanCSS = require('gulp-clean-css');
const sourcemaps = require('gulp-sourcemaps');
const rename = require('gulp-rename');
const  merge = require('merge-stream');

const zip = require('gulp-zip').default;


// scss ----------------------

const scssWatchPath = ['./_src/scss/**/*.scss'];

const cssCopyTo = "./";

function buildScss() { // Parse only the Theme.scss file
  // 1. What scss files to parse?
  var scssCss = gulp.src('./_src/scss/theme.scss')

    .pipe(sourcemaps.init())
    .pipe(sass())
    .pipe(sourcemaps.write(cssCopyTo))
    .pipe(gulp.dest(cssCopyTo));


    var cssCssMin = gulp.src('./_src/scss/theme.scss')


    .pipe(sass())
    .pipe(rename({ suffix: '.min' }))
    .pipe(sourcemaps.init())
    .pipe(sourcemaps.write(cssCopyTo))
    .pipe(cleanCSS({ inline: ['none'] }))
    .pipe(gulp.dest(cssCopyTo));


  return merge(scssCss, cssCssMin);

}



function allTasks() {
  buildScss();
}

function packageSource(cb) {
  var srcPipe = gulp.src(['./**/*.*', "!./theme-source.zip.resources", "!./node_modules/**"])
    .pipe(zip('theme-source.zip.resources'))
    .pipe(gulp.dest('./'))

  cb();

}


// Watch task: watch scss files for changes
// If any change, run scss tasks
function watchTask() {

  gulp.watch(scssWatchPath, gulp.series(buildScss, packageSource));


}

exports.buildScss = buildScss;
exports.source = packageSource;

exports.default = watchTask;

