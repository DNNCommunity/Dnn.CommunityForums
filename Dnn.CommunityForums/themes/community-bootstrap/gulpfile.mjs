import gulp from 'gulp';
import sassModule from 'sass';
import sassPlugin from 'gulp-sass';
import cleanCSS from 'gulp-clean-css';
import sourcemaps from 'gulp-sourcemaps';
import rename from 'gulp-rename';
import merge from 'merge-stream';
import { default as zip } from 'gulp-zip';

const sass = sassPlugin(sassModule);

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

function watchTask() {
  gulp.watch(scssWatchPath, gulp.series(buildScss, packageSource));
}

export { buildScss, packageSource as source, watchTask as default };