import React from "react";
import Pagination from "./Pagination";
import moment from "moment-timezone";

const dataColumnDefs = () => {
  return [
    { dataColumn: "siteName", columnTitle: "Site Name" },
    {
      dataColumn: "userName", columnTitle: "User Name",
      body: {
        format: userNameFormat,
        subData: ["custId"],
      },
    },
    { dataColumn: "currency", columnTitle: "Currency" },
    { dataColumn: "phoneVerified", columnTitle: "Phone Number Verify",
      body: {
        format: verifyStatusFormat,
      },
    },
    { dataColumn: "phoneReward", columnTitle: "Phone Number Reward" },
    {
      dataColumn: "phoneRewardTime", columnTitle: "Phone Number Reward Time (GMT+8)",
      body: {
        format: dateTimeFormat,
      },
    },
    { dataColumn: "emailVerified", columnTitle: "Email Verify",
      body: {
        format: verifyStatusFormat,
      },
    },
    { dataColumn: "emailReward", columnTitle: "Email Reward" },
    {
      dataColumn: "emailRewardTime", columnTitle: "Email Reward Time (GMT+8)",
      body: {
        format: dateTimeFormat,
      },
    },
    { dataColumn: "verificationReawardAmount", columnTitle: "Verification Reaward Amount" },
    {
      dataColumn: "verificationReawardTime", columnTitle: "Verification Reaward Time (GMT+8)",
      body: {
        format: dateTimeFormat,
      },
    }
  ];
};

export default function Result (rewardReport) {
    const columnDefs = dataColumnDefs();
    let headerRows = [];

    columnDefs.forEach((element, idx) => {
        headerRows.push(
        <th
            key={`th-${element.dataColumn}-${idx}`}
            scope="col"
            className={
            element.header && element.header.attr && element.header.attr.class
            ? element.header.attr.class
            : ""}>
            {element.columnTitle}
        </th>);
    });
    let bodyRows = rewardReport.Datas.map((data, i) => {
        let row = columnDefs.map((element, j) => {
            
            let body_content = null;
            if (element.body && element.body.format) {
                let subData = {};
                if(element.body.subData) {
                    element.body.subData.forEach((s) => {
                    subData[s] = data[s];
                    });
                }
                body_content = element.body.format(data[element.dataColumn], subData)

            }
            else {
                body_content = data[element.dataColumn]
            }

            return (
                <td
                key={`td-${element.dataColumn}-${j}`}
                className={
                    element.body && element.body.attr && element.body.attr.class
                    ? element.body.attr.class
                    : ""
                }>
                {body_content}
                </td>
            );
        });

        return <tr key={`result-body-${i}`}>{row}</tr>;
    });

  return (
    <div className="table">
      <div className="table-options">
          <button type="button" className="btn btn-approved" onClick={rewardReport.ExportToExcel}>Export Excel</button>
      </div>
      <table className="table table-striped">
        <thead>
          <tr>{headerRows}</tr>
        </thead>
        <tbody>{bodyRows}</tbody>
      </table>
      <Pagination
        Data={{
          totalPages: rewardReport.PageInfo.totalPages,
          pageNumber: rewardReport.PageInfo.pageNumber,
          rowCount: rewardReport.PageInfo.rowCount,
        }}
        ChangePage={rewardReport.ChangePage}
      />
    </div>
  );
};

const userNameFormat = (userName, subData) => {
  return (
    <a
      href={
        "/CustManage/CustInfo?UserName=" + userName +
        "&custId=" + subData.custId +
        "&TransId="
      }
      target="_blank"
    >
      {userName}
    </a>
  );
};

const dateTimeFormat = (createTime) => {
  return (createTime === null ? "" : moment(createTime).format("l LTS"));
};

const verifyStatusFormat = (isVerified) => {
  return isVerified ? "Yes" : "No"
};