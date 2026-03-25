package com.ssafy.amagetdon.common.interceptor;

import com.ssafy.amagetdon.common.exception.ErrorCode;
import com.ssafy.amagetdon.common.exception.CustomException;
import jakarta.servlet.http.HttpServletRequest;
import jakarta.servlet.http.HttpServletResponse;
import org.springframework.stereotype.Component;
import org.springframework.web.servlet.HandlerInterceptor;

@Component
public class SessionAuthInterceptor implements HandlerInterceptor {

    @Override
    public boolean preHandle(HttpServletRequest request, HttpServletResponse response, Object handler) {
        Object userId = request.getSession(false) == null
                ? null
                : request.getSession(false).getAttribute(SessionKeys.USER_ID);
        if (userId == null) {
            throw new CustomException(ErrorCode.AUTH_REQUIRED);
        }
        return true;
    }
}


