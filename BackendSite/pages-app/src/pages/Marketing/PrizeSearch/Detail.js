import Pagination from "./Pagination";
import React, { useEffect, useRef, useReducer, useState } from "react";
import WebAPI from "../../../module/WebAPI";
import { api } from "../../../module/WebAPI";
import moment from "moment";

const Panel = (props) => {
  const [detailBody, setDetailBody] = useState([]);
  const [stateDatas, setDatas] = useState({
    PageInfo: null,
  });
  const titleData = [
    { dataColumn: "UserName", columnTitle: "UserName" },
    { dataColumn: "DrawTime", columnTitle: "DrawTime" },
  ];
  let headerRows = [];
  titleData.forEach((e, i) => {
    headerRows.push(<th key={`title-${i}`}>{e.columnTitle}</th>);
  });

  useEffect(() => {
    let Data = {
      PrizeId: props.PrizeId,
      EventId: props.EventId,
      StartDate: moment(props.EventTime).unix(),
      EndDate: moment(props.EventTime).add(1, "days").unix() - 1,
      PageNumber: 1,
    };
    api
      .post("/api/BonusManage/GetPrizeLog", Data)
      .then(function (res) {
        if (res.data.errorCode === 0) {
          setDetailBody(res.data.data);
          setDatas({
            PageInfo: {
              totalPages: res.data.totalPages,
              pageNumber: 1,
              rowCount: res.data.rowCount,
            },
          });
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
  }, []);

  const ChangePage = (PageNumber) => {
    let Data = {
      PrizeId: props.PrizeId,
      EventId: props.EventId,
      StartDate: moment(props.EventTime).unix(),
      EndDate: moment(props.EventTime).add(1, "days").unix() - 1,
      PageNumber: PageNumber,
    };
    api
      .post("/api/BonusManage/GetPrizeLog", Data)
      .then(function (res) {
        if (res.data.errorCode === 0) {
          setDetailBody(res.data.data);
          setDatas({
            PageInfo: {
              totalPages: res.data.totalPages,
              pageNumber: PageNumber,
              rowCount: res.data.rowCount,
            },
          });
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
  };

  const bodyRows = detailBody.map((item, index) => {
    return (
      <tr key={index}>
        <td>
          <span className="label">{item.userName}</span>
        </td>
        <td>
          <span className="label">{item.drawTime}</span>
        </td>
      </tr>
    );
  });

  return (
    <div className="table">
      <table className="table table-striped">
        <thead>
          <tr>{headerRows}</tr>
        </thead>
        <tbody>{bodyRows}</tbody>
      </table>
      {stateDatas.PageInfo !== null ? (
        <Pagination
          Data={{
            totalPages: stateDatas.PageInfo.totalPages,
            pageNumber: stateDatas.PageInfo.pageNumber,
            rowCount: stateDatas.PageInfo.rowCount,
          }}
          ChangePage={ChangePage}
        />
      ) : null}
    </div>
  );
};

export default Panel;
