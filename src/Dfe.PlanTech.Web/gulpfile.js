/// <binding ProjectOpened='js:watch, sass-to-min-css:watch' />
"use strict";

var gulp = require("gulp"),
    sass = require('gulp-sass')(require('sass')),
    sourcemaps = require('gulp-sourcemaps'),
    csso = require('gulp-csso');

gulp.task('sass-to-min-css', async function () {
    return gulp.src('./Styles/scss/application.scss')
        .pipe(sourcemaps.init())
        .pipe(sass().on('error', sass.logError))
        .pipe(csso())
        .pipe(sourcemaps.write('.'))
        .pipe(gulp.dest('./wwwroot/css'));
});

gulp.task('sass-to-min-css:watch', function () {
    gulp.watch('./Styles/**', gulp.series('sass-to-min-css'));
});


gulp.task("default", gulp.series("sass-to-min-css"));