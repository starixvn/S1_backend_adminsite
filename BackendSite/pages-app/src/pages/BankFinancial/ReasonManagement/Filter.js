import React from "react";
import moment from "moment";
import DateTimePicker from "react-datetime";

const Panel = (props) => {
  let siteListOptions = [];
  if (props.SiteList) {
    siteListOptions = props.SiteList.map((i) => {
      return {
        value: Number(i.siteId),
        displayText: i.siteName,
      };
    });
  }
  return (
      
  )
};
