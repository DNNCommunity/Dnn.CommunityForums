const gulp = require('gulp');
const less = require('gulp-less');
const cleanCSS = require('gulp-clean-css');
const sourcemaps = require('gulp-sourcemaps');
const rename = require('gulp-rename');

const zip = require('gulp-zip');



// LESS ----------------------

const lessWatchPath = ['./_less/**/*.less'];

const cssCopyTo = [
  "./"

]

function buildLess() { // Parse only the Skin.less file
  // 1. What less files to parse?
  var lessPipe = gulp.src('./_less/theme.less')


    // 2. Init Source maps
    .pipe(sourcemaps.init())

    // 3. Parse Less
    .pipe(less())

    // 4. Compress CSS
    .pipe(cleanCSS({ inline: ['none'] }))

    // 4. CreateSource maps
    .pipe(sourcemaps.write('../../'))

  // Loop over destinations to copy the css to
  cssCopyTo.forEach(function (d) {
    lessPipe = lessPipe.pipe(gulp.dest(d));
  });

  return lessPipe;

}



function styleTask() {
  buildLess();
}

function packageSource(cb) {
  var srcPipe = gulp.src(['./_less**/**/*.*', "gulpfile.js", "*.json"])
	.pipe(zip('theme-source.zip.resources'))
	.pipe(gulp.dest('./'))

  cb();
  
}


// Watch task: watch LESS files for changes
// If any change, run LESS tasks
function watchTask() {
  gulp.watch(lessWatchPath, buildLess, packageSource);


}

exports.buildLess = buildLess;
exports.style = styleTask;
exports.source = packageSource;

exports.default = watchTask;

