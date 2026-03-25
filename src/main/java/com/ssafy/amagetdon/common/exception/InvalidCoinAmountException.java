package com.ssafy.amagetdon.common.exception;

public class InvalidCoinAmountException extends RuntimeException {
    public InvalidCoinAmountException(String message) {
        super(message);
    }
}