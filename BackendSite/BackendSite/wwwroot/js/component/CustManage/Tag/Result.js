const Result = (props) => {
  let rows =
    props.Data.length === 0
      ? null
      : props.Data.map((d, dIdx) => {
          let tag = props.TagList.find((x) => x.id === d);
          if (tag != undefined) {
            return (
              <tr key={`result-${dIdx}`}>
                <td>{tag.tagName}</td>
                <td bgcolor={tag.tagColour}>{tag.tagColour}</td>
                <td>{tag.description}</td>
              </tr>
            );
          }
        });
  return (
    <>
      <div className="table-responsive-md table">
        <span class="label">Added Tag List</span>
        <table className="table table-striped">
          <thead>
            <tr>
              <th scope="col">TagName</th>
              <th scope="col">TagColour</th>
              <th scope="col">Description</th>
            </tr>
          </thead>
          {rows}
        </table>
      </div>
    </>
  );
};
export default Result;
