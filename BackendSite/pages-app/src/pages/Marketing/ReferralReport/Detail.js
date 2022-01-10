import Pagination from "./Pagination";
import React, { useEffect, useRef, useReducer, useState } from "react";
import WebAPI from "../../../module/WebAPI";
import { api } from "../../../module/WebAPI";
import moment from "moment";

const Panel = (props) => {
  const [detailBody, setDetailBody] = useState([]);
  const [stateDatas, setDatas] = useState({
    PageInfo: null
  });
  const titleData = [
    { dataColumn: "UserName", columnTitle: "UserName" },
    { dataColumn: "Currency", columnTitle: "Currency" },
    { dataColumn: "Reg Time", columnTitle: "Reg Time" },
    { dataColumn: "TotalDeposit", columnTitle: "TotalDeposit" },
    { dataColumn: "TotalBet", columnTitle: "TotalBet" },
    { dataColumn: "Reward Amount", columnTitle: "Reward Amount" },
    { dataColumn: "Reward Time", columnTitle: "Reward Time" },
    { dataColumn: "Status", columnTitle: "Status" },
  ];
  let headerRows = [];
  titleData.forEach((e, i) => {
    headerRows.push(<th key={`title-${i}`}>{e.columnTitle}</th>);
  });

  useEffect(() => {
    let Data = {
      CustId: props.CustId,
      StartDate: props.Filter.StartDate === "" ? null : moment(props.Filter.StartDate).format("YYYY-MM-DD") ,
      EndDate: props.Filter.EndDate === "" ? null : moment(props.Filter.EndDate).format("YYYY-MM-DD"),
      PageNumber: 1,
    };
    api
      .post("/api/ReferralManagement/GetRefFriendList", Data)
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
        CustId: props.CustId,
        StartDate:props.StartDate,
        EndDate:props.EndDate,
        PageNumber: PageNumber,
      };
      api
        .post("/api/ReferralManagement/GetRefFriendList", Data)
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
          <span className="label">{item.currency}</span>
        </td>
        <td>
          <span className="label">{moment(item.registerDate).format("l LTS")}</span>
        </td>
        <td>
          <span className="label">
            {item.totalDeposit}/{item.depositC}
          </span>
        </td>
        <td>
          <span className="label">
            {item.totalBet}/{item.vaildBetC}
          </span>
        </td>
        <td>
          <span className="label">{item.bonusAmount}</span>
        </td>
        <td>
          <span className="label">{ item.rewardTime != null ? moment(item.rewardTime).format("l LTS") : null }</span>
        </td>
        <td>
          <span className="label">{item.statusName}</span>
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
