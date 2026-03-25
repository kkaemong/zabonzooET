package com.ssafy.amagetdon.common.exception;

public class InsufficientCoinException extends RuntimeException {
    public InsufficientCoinException(String message) {
        super(message);
    }
}