const gulp = require('gulp');
const less = require('gulp-less');
const cleanCSS = require('gulp-clean-css');
const sourcemaps = require('gulp-sourcemaps');
const rename = require('gulp-rename');


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
    .pipe(sourcemaps.write('../'))

  // Loop over destinations to copy the css to
  cssCopyTo.forEach(function (d) {
    lessPipe = lessPipe.pipe(gulp.dest(d));
  });

  return lessPipe;

}





function styleTask() {
  buildLess();

}


// Watch task: watch SCSS and JS files for changes
// If any change, run scss and js tasks simultaneously
function watchTask() {
  gulp.watch(lessWatchPath, buildLess);

}

exports.buildLess = buildLess;
exports.style = styleTask;

exports.default = watchTask;

