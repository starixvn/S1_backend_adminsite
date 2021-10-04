import { dateFormat, boolFormat } from "./dataDefs";

const contentFormat = (v, id) => {
  // return v.split("\n").map((d, idx)=>{
  //     return <p style={{marginBottom:'0'}} key={`${id}-content-${idx}`}>{d}</p>
  // })
  return <>{v}</>;
};

const columnDefs = [
  {
    dataColumn: "createTime",
    columnTitle: `CreateTime (Adjusted to GMT+8)`,
    body: {
      format: dateFormat,
    },
  },
  { dataColumn: "sender", columnTitle: "Sender" },
  {
    dataColumn: "flag",
    columnTitle: "Read",
    body: {
      format: boolFormat,
    },
  },
  { dataColumn: "title", columnTitle: "Subject" },
  {
    dataColumn: "content",
    columnTitle: "Content",
    body: {
      format: contentFormat,
    },
  },
];

const Panel = (props) => {
  let data = props.data;
  return (
    <div>
      <button
        type="button"
        className="btn btn-back"
        onClick={props.ClickBack}
        style={{ marginBottom: "1rem" }}
      >
        Back
      </button>
      {columnDefs.map((col, idx) => {
        return col.dataColumn !== "content" ? (
          <div key={`detail-${idx}`} style={{ marginBottom: "1rem" }}>
            <p style={{ marginBottom: "0rem" }}>
              <b>{col.columnTitle}:</b>
            </p>
            {col.body && col.body.format
              ? col.body.format(data[col.dataColumn], col.id)
              : data[col.dataColumn]}
          </div>
        ) : (
          <div key={`detail-${idx}`} style={{ marginBottom: "1rem" }}>
            <p style={{ marginBottom: "0rem" }}>
              <b>{col.columnTitle}:</b>
            </p>
            <div dangerouslySetInnerHTML={{__html: data[col.dataColumn]}}></div>
            {/* {data[col.dataColumn]} */}
          </div>
        );
      })}
    </div>
  );
};

export default Panel;
