var gulp = require('gulp');  
var sourcemaps = require('gulp-sourcemaps');  
var ts = require('gulp-typescript');  
var babel = require('gulp-babel');
var uglify = require('gulp-uglify');
var tsProject = ts.createProject('./tsconfig.json');

gulp.task('default', function() {
    gulp.src('./node_modules/gulp-babel/node_modules/babel-core/browser-polyfill.min.js')
        .pipe(gulp.dest('.'));
      
    return gulp.src('asl4.ts')
        .pipe(sourcemaps.init())
        .pipe(ts(tsProject))
        .pipe(babel())
        //.pipe(uglify())
        .pipe(sourcemaps.write('.'))
        .pipe(gulp.dest('.'));
});